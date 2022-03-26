using Common.Netcode;
using GameServer.Console;
using GameServer.Server;
using GameServer.Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Utils
{
    public static class PlayerUtils
    {
        public static void SaveAllOnlinePlayersToDatabase()
        {
            foreach (var player in ENetServer.Players.Values)
            {
                player.SaveConfig();
            }

            Logger.Log($"Saved {ENetServer.Players.Count} online players to the database");
        }

        public static ServerPlayer GetConfig(string username)
        {
            var playerNames = FileManager.GetAllConfigNamesInFolder("Players");
            var playerName = playerNames.Find(str => str == username);

            if (playerName == null)
                return null;

            return FileManager.ReadConfig<ServerPlayer>($"Players/{playerName}");
        }

        public static List<ServerPlayer> GetAllConfigs()
        {
            var playerConfigs = new List<ServerPlayer>();
            var playerNames = FileManager.GetAllConfigNamesInFolder("Players");
            foreach (var playerName in playerNames)
            {
                playerConfigs.Add(FileManager.ReadConfig<ServerPlayer>($"Players/{playerName}"));
            }
            return playerConfigs;
        }

        public static void Ban(string username)
        {
            // First check if the player is online and try to ban them
            if (BanOnline(username))
                return;

            // The player is not online, see if they joined before and if so ban them
            BanOffline(username);
        }

        private static bool BanOnline(string username)
        {
            ServerPlayer onlinePlayer = null;
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

        private static bool BanOffline(string username)
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
            var playerConfigs = GetAllConfigs();
            ServerPlayer player = null;

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
        public static void PardonOffline(string username)
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
    }

    public class BannedPlayer
    {
        public string Ip { get; set; }
        public string Name { get; set; }
    }
}
