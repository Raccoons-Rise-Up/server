using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.IO;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Server.Packets;
using GameServer.Database;
using GameServer.Logging;
using GameServer.Utilities;

namespace GameServer.Server
{
    public class ENetServer
    {
        public static ConcurrentBag<Event> Incoming { get; private set; }
        public static ConcurrentQueue<ServerInstructions> ServerInstructions { get; private set; }
        public static List<Player> Players { get; private set; }
        public static Dictionary<ClientPacketOpcode, HandlePacket> HandlePackets { get; private set; }

        public static byte ServerVersionMajor { get; private set; }
        public static byte ServerVersionMinor { get; private set; }
        public static byte ServerVersionPatch { get; private set; }

        #region WorkerThread
        public static void WorkerThread() 
        {
            Thread.CurrentThread.Name = "SERVER";

            Utils.CreateJSONDictionaryFile("banned_players");

            // Server Version
            ServerVersionMajor = 0;
            ServerVersionMinor = 1;
            ServerVersionPatch = 0;

            Incoming = new();
            ServerInstructions = new();
            Players = new();

            HandlePackets = typeof(HandlePacket).Assembly.GetTypes()
                .Where(x => typeof(HandlePacket)
                .IsAssignableFrom(x) && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<HandlePacket>()
                .ToDictionary(x => x.Opcode, x => x);

            Library.Initialize();

            var maxClients = 100;
            ushort port = 25565;

            using (var server = new Host())
            {
                var address = new Address
                {
                    Port = port
                };

                server.Create(address, maxClients);

                Logger.Log($"Listening on port {port}");

                while (!Console.KeyAvailable)
                {
                    var polled = false;

                    // Server Instructions
                    while (ServerInstructions.TryDequeue(out ServerInstructions result))
                    {
                        foreach (var cmd in result.Instructions)
                        {
                            var opcode = cmd.Key;

                            if (opcode == ServerInstructionOpcode.PardonPlayer) 
                            {
                                var username = cmd.Value[0].ToString();

                                Utils.PardonOfflinePlayer(username);
                            }

                            if (opcode == ServerInstructionOpcode.BanPlayer) 
                            {
                                var username = cmd.Value[0].ToString();

                                var bannedOnline = Utils.BanOnlinePlayer(username);

                                if (!bannedOnline)
                                    Utils.BanOfflinePlayer(username);
                            }

                            if (opcode == ServerInstructionOpcode.KickPlayer) 
                            {
                                var username = cmd.Value[0].ToString();
                                var player = Players.Find(x => x.Username == username);
                                if (player == null) 
                                {
                                    Logger.Log($"No player with the username '{username}' is online");
                                    break;
                                }

                                player.Peer.DisconnectNow((uint)DisconnectOpcode.Kicked);
                                Players.Remove(player);
                                Logger.Log($"Player '{player.Username}' was kicked");
                            }

                            if (opcode == ServerInstructionOpcode.GetOnlinePlayers) 
                            {
                                if (Players.Count == 0) 
                                {
                                    Logger.Log("There are 0 players on the server");
                                    break;
                                }

                                Logger.LogRaw($"\nOnline Players: {string.Join(' ', Players)}");
                            }

                            if (opcode == ServerInstructionOpcode.GetPlayerStats) 
                            {
                                var player = Players.Find(x => x.Username == cmd.Value[0].ToString());
                                if (player == null)
                                    break;

                                AddGoldGeneratedFromStructures(player);

                                var diff = DateTime.Now - player.LastSeen;
                                var diffReadable = $"Days: {diff.Days}, Hours: {diff.Hours}, Minutes: {diff.Minutes}, Seconds: {diff.Seconds}";

                                Logger.LogRaw(
                                    $"\n\nCACHE" +
                                    $"\nUsername: {player.Username} " +
                                    $"\nGold: {player.Gold}" +
                                    $"\nStructure Huts: {player.StructureHut}" +
                                    $"\nLast Seen: {diffReadable}"
                                );
                            }

                            if (opcode == ServerInstructionOpcode.ClearPlayerStats) 
                            {
                                var player = Players.Find(x => x.Username == cmd.Value[0].ToString());
                                if (player != null)
                                {
                                    player.ResetValues();

                                    Logger.Log($"Cleared {player.Username} from list");
                                }
                            }
                        }
                    }

                    // Incoming
                    while (Incoming.TryTake(out Event netEvent))
                    {
                        var peer = netEvent.Peer;
                        var packetSizeMax = 1024;
                        var readBuffer = new byte[packetSizeMax];
                        var packetReader = new PacketReader(readBuffer);
                        packetReader.BaseStream.Position = 0;

                        netEvent.Packet.CopyTo(readBuffer);

                        var opcode = (ClientPacketOpcode)packetReader.ReadByte();

                        HandlePackets[opcode].Handle(netEvent, ref packetReader);

                        packetReader.Dispose();
                        netEvent.Packet.Dispose();
                    }

                    while (!polled)
                    {
                        if (server.CheckEvents(out Event netEvent) <= 0)
                        {
                            if (server.Service(15, out netEvent) <= 0)
                                break;

                            polled = true;
                        }

                        var eventType = netEvent.Type;

                        if (eventType == EventType.None) 
                        {
                            Logger.LogWarning("Received EventType.None");
                        }

                        if (eventType == EventType.Connect) 
                        {
                            var bannedPlayers = Utils.ReadJSONFile<Dictionary<string, BannedPlayer>>("banned_players");

                            if (bannedPlayers.ContainsKey(netEvent.Peer.IP)) 
                            {
                                var bannedPlayer = bannedPlayers[netEvent.Peer.IP];

                                netEvent.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);
                                Logger.Log($"Player '{bannedPlayer.Name}' tried to join but is banned");
                            }
                            else 
                            {
                                netEvent.Peer.Timeout(32, 1000, 4000);
                            }
                        }

                        if (eventType == EventType.Disconnect) 
                        {
                            var player = Players.Find(x => x.Peer.ID == netEvent.Peer.ID);

                            SavePlayerToDatabase(player);

                            // Remove player from player list
                            Players.Remove(player);

                            Logger.Log($"Player '{(player == null ? netEvent.Peer.ID : player.Username)}' disconnected");
                        }

                        if (eventType == EventType.Timeout) 
                        {
                            var player = Players.Find(x => x.Peer.ID == netEvent.Peer.ID);

                            SavePlayerToDatabase(player);

                            // Remove player from player list
                            Players.Remove(player);

                            Logger.Log($"Player '{(player == null ? netEvent.Peer.ID : player.Username)}' timed out");
                        }

                        if (eventType == EventType.Receive) 
                        {
                            //Logger.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                            Incoming.Add(netEvent);
                        }
                    }
                }

                server.Flush();
            }

            Library.Deinitialize();
        }

        public static void Send(GamePacket gamePacket, Peer peer, PacketFlags packetFlags)
        {
            // Send data to a specific client (peer)
            var packet = default(Packet);
            packet.Create(gamePacket.Data, packetFlags);
            byte channelID = 0;
            peer.Send(channelID, ref packet);
        }
        
        public static void SavePlayerToDatabase(Player player) 
        {
            using var db = new DatabaseContext();

            var playerExistsInDatabase = false;

            AddGoldGeneratedFromStructures(player);

            foreach (var dbPlayer in db.Players.ToList()) 
            {
                if (player.Username == dbPlayer.Username) 
                {
                    playerExistsInDatabase = true;

                    UpdatePlayerValuesInDatabase(dbPlayer, player);
                    break;
                }
            }

            if (!playerExistsInDatabase) 
            {
                db.Add((ModelPlayer)player);
            }

            db.SaveChanges();
        }

        public static void SaveAllPlayersToDatabase()
        {
            if (Players.Count == 0)
                return;

            Logger.Log($"Saving {Players.Count} players to the database");

            using var db = new DatabaseContext();

            var playersThatAreNotInDatabase = new List<Player>();

            foreach (var player in Players)
            {
                foreach (var dbPlayer in db.Players.ToList())
                {
                    if (player.Username == dbPlayer.Username)
                    {
                        AddGoldGeneratedFromStructures(player);
                        UpdatePlayerValuesInDatabase(dbPlayer, player);
                        break;
                    }

                    playersThatAreNotInDatabase.Add(player);
                }
            }

            foreach (var player in playersThatAreNotInDatabase)
            {
                AddGoldGeneratedFromStructures(player);
                db.Add((ModelPlayer)player);
            }

            db.SaveChanges();
        }

        private static void UpdatePlayerValuesInDatabase(ModelPlayer dbPlayer, Player player) 
        {
            dbPlayer.Ip = player.Ip;
            dbPlayer.Gold = player.Gold;
            dbPlayer.StructureHut = player.StructureHut;
            dbPlayer.LastCheckStructureHut = DateTime.Now;
            dbPlayer.LastSeen = DateTime.Now;
        }
        #endregion

        public static void AddGoldGeneratedFromStructures(Player player) 
        {
            // Calculate players new gold value based on how many structures they own
            var diff = DateTime.Now - player.LastCheckStructureHut;
            uint goldGenerated = player.StructureHut * (uint)diff.TotalSeconds;

            player.LastCheckStructureHut = DateTime.Now;

            player.Gold += goldGenerated;
        }
    }

    public struct PlayerValues 
    {
        public uint Gold { get; set; }
        public uint StructureHuts { get; set; }
    }

    public static class StartingValues
    {
        public const uint Gold = 100;
        public const uint StructureHuts = 0;
    }

    public class ServerInstructions 
    {
        public Dictionary<ServerInstructionOpcode, List<object>> Instructions { get; set; }

        public ServerInstructions()
        {
            Instructions = new Dictionary<ServerInstructionOpcode, List<object>>();
        }

        public ServerInstructions(ServerInstructionOpcode opcode)
        {
            Instructions = new Dictionary<ServerInstructionOpcode, List<object>>
            {
                [opcode] = null
            };
        }

        public void Set(ServerInstructionOpcode opcode, params object[] data)
        {
            Instructions[opcode] = new List<object>(data);
        }
    }

    public enum ServerInstructionOpcode 
    {
        GetOnlinePlayers,
        GetPlayerStats,
        KickPlayer,
        BanPlayer,
        PardonPlayer,
        ClearPlayerStats
    }
}
