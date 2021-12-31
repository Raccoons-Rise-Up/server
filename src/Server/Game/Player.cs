using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ENet;
using Common.Game;
using Common.Utils;
using GameServer.Logging;
using GameServer.Utilities;
using Common.Networking.Packet;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class Player : User
    {
        [JsonIgnore] public Peer Peer { get; set; }
        public string Ip { get; set; }
        public DateTime LastSeen { get; set; }
        public Dictionary<ResourceType, double> ResourceCounts { get; set; }
        public Dictionary<StructureType, uint> StructureCounts { get; set; }
        public DateTime StructuresLastChecked { get; set; }
        [JsonIgnore] public bool InGame { get; set; }

        public Player(Peer peer) : base()
        {
            Peer = peer;
            if (peer.IsSet)
                Ip = peer.IP;

            LastSeen = DateTime.Now;

            ResourceCounts = new();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                ResourceCounts.Add(type, 0);

            StructureCounts = new();
            foreach (StructureType type in Enum.GetValues(typeof(StructureType)))
                StructureCounts.Add(type, 0);

            StructuresLastChecked = new();
        }

        public void ResetValues() 
        {
            var resourceTypes = SharedUtils.GetEnumList<ResourceType>();
            foreach (var resourceType in resourceTypes)
                ResourceCounts[resourceType] = 0;

            var structureTypes = SharedUtils.GetEnumList<StructureType>();
            foreach (var structureType in structureTypes)
                StructureCounts[structureType] = 0;

            UpdatePlayerConfig();

            var cmd = new ENetCmds();
            cmd.Set(ServerOpcode.SendPlayerData, Username);

            ENetServer.ENetCmds.Enqueue(cmd);
        }

        public PurchaseResult TryPurchase(StructureInfo structure)
        {
            AddResourcesGeneratedFromStructures();

            // Return the resources that the player is lacking in order to make this purchase
            var lackingResources = GetLackingResources(structure);
            if (lackingResources.Count > 0)
                return new PurchaseResult
                {
                    Result = PurchaseEnumResult.LackingResources,
                    Resources = lackingResources
                };

            // The player has enough to purchase this
            var newPlayerResources = new Dictionary<ResourceType, uint>();

            foreach (var resource in ResourceCounts)
            {
                if (structure.Cost.TryGetValue(resource.Key, out uint resourceCost))
                {
                    ResourceCounts[resource.Key] -= resourceCost;
                    newPlayerResources.Add(resource.Key, (uint)ResourceCounts[resource.Key]);
                }
            }

            // Need to ensure all the resources are being updated (not just for cost resources but for production resources too)
            foreach (var resourceKey in structure.Production.Keys) 
            {
                if (!newPlayerResources.ContainsKey(resourceKey)) 
                    newPlayerResources.Add(resourceKey, (uint)ResourceCounts[resourceKey]);
            }

            StructureCounts[structure.Type] += 1;

            // Return the now updated player resource values based on this new purchase
            return new PurchaseResult
            {
                Result = PurchaseEnumResult.Success,
                Resources = newPlayerResources
            };
        }

        public void AddResourcesGeneratedFromStructures() 
        {
            foreach (var structureCount in StructureCounts) 
            {
                if (structureCount.Value == 0)
                    continue;

                var structureData = ENetServer.StructureInfoData[structureCount.Key];

                foreach (var prod in structureData.Production) 
                {
                    var timeDiff = DateTime.Now - StructuresLastChecked;
                    var amountGenerated = prod.Value * structureCount.Value * timeDiff.TotalSeconds;

                    ResourceCounts[prod.Key] += amountGenerated;
                }
            }

            StructuresLastChecked = DateTime.Now;
        }

        public Dictionary<ResourceType, uint> GetLackingResources(StructureInfo structure)
        {
            var lackingResources = new Dictionary<ResourceType, uint>();
            foreach (var resource in ResourceCounts)
            {
                var playerResourceKey = resource.Key;
                var playerResourceAmount = resource.Value;

                if (structure.Cost.TryGetValue(playerResourceKey, out uint structureResourceValue)) 
                {
                    if (playerResourceAmount < structureResourceValue)
                    {
                        lackingResources.Add(playerResourceKey, (uint)(structureResourceValue - playerResourceAmount));
                    }
                }
            }

            return lackingResources;
        }

        public void UpdatePlayerConfig()
        {
            // Check if player exists in configs
            var dbPlayers = FileManager.GetAllConfigNamesInFolder("Players");
            var dbPlayer = dbPlayers.Find(str => str == Username);

            // Reminder: WriteConfig creates a config if no file exists
            FileManager.WriteConfig($"Players/{Username}", this);

            if (dbPlayer == null)
            {
                // Player does not exist in database, lets add them to the database
                Logger.Log($"Player '{Username}' config created");
                return;
            }

            // Player exists in the database, lets update the config
            Logger.Log($"Player '{Username}' config updated");
        }

        public static Player GetPlayerConfig(string username)
        {
            var playerNames = FileManager.GetAllConfigNamesInFolder("Players");
            var playerName = playerNames.Find(str => str == username);

            if (playerName == null)
                return null;

            return FileManager.ReadConfig<Player>($"Players/{playerName}");
        }

        public static List<Player> GetAllPlayerConfigs()
        {
            var playerConfigs = new List<Player>();
            var playerNames = FileManager.GetAllConfigNamesInFolder("Players");
            foreach (var playerName in playerNames)
            {
                playerConfigs.Add(FileManager.ReadConfig<Player>($"Players/{playerName}"));
            }
            return playerConfigs;
        }

        public static void BanPlayer(string username)
        {
            // First check if the player is online and try to ban them
            if (BanOnlinePlayer(username))
                return;

            // The player is not online, see if they joined before and if so ban them
            BanOfflinePlayer(username);
        }

        private static bool BanOnlinePlayer(string username)
        {
            Player onlinePlayer = null;
            foreach (var p in ENetServer.Players.Values)
            {
                if (p.Username == username)
                    onlinePlayer = p;
            }

            // Player is not online
            if (onlinePlayer == null)
                return false;

            // Player is online, disconnect them immediately and remove them from the player cache
            onlinePlayer.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);
            ENetServer.Players.Remove(onlinePlayer.Peer.ID);

            // Add the player to the banlist
            var bannedPlayers = FileManager.ReadConfig<List<BannedPlayer>>("banned_players");
            bannedPlayers.Add(new BannedPlayer
            {
                Name = onlinePlayer.Username,
                Ip = onlinePlayer.Ip
            });

            FileManager.WriteConfig("banned_players", bannedPlayers);

            Logger.Log($"Player '{username}' has been banned");

            return true;
        }

        private static bool BanOfflinePlayer(string username)
        {
            var bannedPlayers = FileManager.ReadConfig<List<BannedPlayer>>("banned_players");
            var bannedPlayer = bannedPlayers.Find(x => x.Name.Equals(username));

            // The player exists in the banlist already
            if (bannedPlayer != null)
            {
                Logger.Log($"Player '{username}' was banned already");
                return true;
            }

            // Check if the player has joined the server before
            var playerConfigs = Player.GetAllPlayerConfigs();
            Player player = null;

            foreach (var playerConfig in playerConfigs)
                if (playerConfig.Username.Equals(username))
                    player = playerConfig;

            // Player has never played before
            if (player == null)
            {
                Logger.Log($"No such player with the username '{username}' exists");
                return false;
            }

            // Player has played before, add them to the banlist
            bannedPlayers.Add(new BannedPlayer
            {
                Name = player.Username,
                Ip = player.Ip
            });
            FileManager.WriteConfig("banned_players", bannedPlayers);

            Logger.Log($"Player '{username}' has been banned");

            return true;
        }

        // Note that there is no method called "PardonOnlinePlayer(string username)" as this would not make sense because a banned player can never be online
        public static void PardonOfflinePlayer(string username)
        {
            var bannedPlayers = FileManager.ReadConfig<List<BannedPlayer>>("banned_players");
            var bannedPlayer = bannedPlayers.Find(x => x.Name.Equals(username));

            // No such player with the username exists
            if (bannedPlayer == null)
            {
                Logger.Log($"Player '{username}' could be found");
                return;
            }

            // Pardon the player, remove them from the banlist
            bannedPlayers.Remove(bannedPlayer);

            FileManager.WriteConfig("banned_players", bannedPlayers);

            Logger.Log($"Player '{bannedPlayer.Name}' was pardoned");
        }

        public override string ToString()
        {
            return Username;
        }
    }

    // This structure is stored in JSON format in banned_players.json
    public class BannedPlayer
    {
        public string Ip { get; set; }
        public string Name { get; set; }
    }

    public struct PurchaseResult 
    {
        public Dictionary<ResourceType, uint> Resources { get; set; }
        public PurchaseEnumResult Result { get; set; }
    }

    public enum PurchaseEnumResult 
    {
        LackingResources,
        Success
    }
}
