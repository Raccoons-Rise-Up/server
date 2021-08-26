using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Server.Packets;
using GameServer.Database;
using GameServer.Logging;

namespace GameServer.Server
{
    public class BannedPlayer 
    {
        public string Name { get; set; }
        public string Ip { get; set; }
    }

    public class ENetServer
    {
        public static ConcurrentQueue<ServerInstructions> ServerInstructions { get; private set; }
        public static List<Player> Players { get; private set; }

        public static byte ServerVersionMajor { get; private set; }
        public static byte ServerVersionMinor { get; private set; }
        public static byte ServerVersionPatch { get; private set; }
        private static string PathToRes { get; set; }

        private static List<T> ReadJSONFile<T>(string filename) 
        {
            var pathToFile = Path.Combine(PathToRes, filename + ".json");

            var text = File.ReadAllText(pathToFile);
            return JsonSerializer.Deserialize<List<T>>(text);
        }

        private static void WriteToJSONFile<T>(string filename, List<T> list) 
        {
            var pathToFile = Path.Combine(PathToRes, filename + ".json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            File.WriteAllText(pathToFile, JsonSerializer.Serialize(list, options));
        }

        private static void CreateJSONFile(string filename) 
        {
            var pathToFile = Path.Combine(PathToRes, filename + ".json");

            if (!File.Exists(pathToFile))
            {
                var fs = File.Create(pathToFile);
                fs.Close();
            }

            // Make sure json file has valid json tokens
            if (File.ReadAllText(pathToFile) == "") // Only write if nothing in file (note that "" is returned if nothing is in the file)
            {
                string json = JsonSerializer.Serialize(new List<string>());
                File.WriteAllText(pathToFile, json);
            }
        }

        #region WorkerThread
        public static void WorkerThread() 
        {
            Thread.CurrentThread.Name = "SERVER";

            PathToRes = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"res");

            CreateJSONFile("banned_players");

            // Server Version
            ServerVersionMajor = 0;
            ServerVersionMinor = 1;
            ServerVersionPatch = 0;

            ServerInstructions = new();
            Players = new();

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

                            if (opcode == ServerInstructionOpcode.BanPlayer) 
                            {
                                var username = cmd.Value[0].ToString();
                                var player = Players.Find(x => x.Username == username);
                                if (player == null)
                                {
                                    Logger.Log($"No player with the username '{username}' is online");
                                    break;
                                }

                                player.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);

                                var bannedPlayers = ReadJSONFile<BannedPlayer>("banned_players");
                                if (bannedPlayers.Exists(x => x.Ip == player.Ip))
                                {
                                    Logger.Log($"Player '{player.Username}' is in banned list already, not writing to banned list");
                                }
                                else 
                                {
                                    bannedPlayers.Add(new BannedPlayer()
                                    {
                                        Name = player.Username,
                                        Ip = player.Ip
                                    });

                                    WriteToJSONFile("banned_players", bannedPlayers);
                                }

                                Players.Remove(player);
                                Logger.Log($"Player '{player.Username}' was banned");
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
                            var bannedPlayers = ReadJSONFile<BannedPlayer>("banned_players");
                            var player = bannedPlayers.Find(x => x.Ip == netEvent.Peer.IP);
                            if (player != null)
                            {
                                netEvent.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);
                                Logger.Log($"Player '{player.Name}' tried to join but is banned");
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

                            HandlePacket(ref netEvent);
                            netEvent.Packet.Dispose();
                        }
                    }
                }

                server.Flush();
            }

            Library.Deinitialize();
        }

        private static void Send(GamePacket gamePacket, Peer peer, PacketFlags packetFlags)
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

                    SavePlayerValuesToDatabase(dbPlayer, player);
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
                        SavePlayerValuesToDatabase(dbPlayer, player);
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

        private static void SavePlayerValuesToDatabase(ModelPlayer dbPlayer, Player player) 
        {
            dbPlayer.Gold = player.Gold;
            dbPlayer.StructureHut = player.StructureHut;
            dbPlayer.LastCheckStructureHut = DateTime.Now;
            dbPlayer.LastSeen = DateTime.Now;
        }
        #endregion

        private static void HandlePacket(ref Event netEvent) 
        {
            var peer = netEvent.Peer;
            var packetSizeMax = 1024;
            var readBuffer = new byte[packetSizeMax];
            var packetReader = new PacketReader(readBuffer);
            packetReader.BaseStream.Position = 0;

            netEvent.Packet.CopyTo(readBuffer);

            var opcode = (ClientPacketOpcode)packetReader.ReadByte();

            if (opcode == ClientPacketOpcode.Login)
            {
                var data = new RPacketLogin();
                data.Read(packetReader);

                ClientPacketHandleLogin(data, peer);
            }

            if (opcode == ClientPacketOpcode.PurchaseItem)
            {
                var data = new RPacketPurchaseItem();
                data.Read(packetReader);

                ClientPacketHandlePurchaseItem(data, peer);
            }

            packetReader.Dispose();
        }

        #region ClientPacketHandleLogin
        private static void ClientPacketHandleLogin(RPacketLogin data, Peer peer) 
        {
            // Check if versions match
            if (data.VersionMajor != ServerVersionMajor || data.VersionMinor != ServerVersionMinor ||
                data.VersionPatch != ServerVersionPatch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ServerVersionMajor}.{ServerVersionMinor}.{ServerVersionPatch}";

                Logger.Log($"Player '{data.Username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packetData = new WPacketLogin 
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    VersionMajor = ServerVersionMajor,
                    VersionMinor = ServerVersionMinor,
                    VersionPatch = ServerVersionPatch,
                };

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData);
                Send(packet, peer, PacketFlags.Reliable);

                return;
            }

            // Check if username exists in database
            using var db = new DatabaseContext();

            var dbPlayer = db.Players.ToList().Find(x => x.Username == data.Username);

            // These values will be sent to the client
            var playerValues = new PlayerValues();

            if (dbPlayer != null)
            {
                // RETURNING PLAYER

                playerValues.Gold = dbPlayer.Gold;
                playerValues.StructureHuts = dbPlayer.StructureHut;

                // Add the player to the list of players currently on the server
                var player = new Player
                {
                    Gold = dbPlayer.Gold,
                    StructureHut = dbPlayer.StructureHut,
                    LastCheckStructureHut = dbPlayer.LastCheckStructureHut,
                    LastSeen = DateTime.Now,
                    Username = dbPlayer.Username,
                    Peer = peer
                };

                Players.Add(player);

                Logger.Log($"Player '{data.Username}' logged in");
            }
            else
            {
                // NEW PLAYER

                playerValues.Gold = StartingValues.Gold;
                playerValues.StructureHuts = StartingValues.StructureHuts;

                // Add the player to the list of players currently on the server
                Players.Add(new Player
                {
                    Peer = peer,
                    Username = data.Username,
                    Gold = StartingValues.Gold,
                    LastSeen = DateTime.Now
                });

                Logger.Log($"User '{data.Username}' logged in for the first time");
            }

            {
                var packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccess,
                    Gold = playerValues.Gold,
                    StructureHuts = playerValues.StructureHuts
                };

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData);
                Send(packet, peer, PacketFlags.Reliable);
            }
        }
        #endregion

        private static void AddGoldGeneratedFromStructures(Player player) 
        {
            // Calculate players new gold value based on how many structures they own
            var diff = DateTime.Now - player.LastCheckStructureHut;
            uint goldGenerated = player.StructureHut * (uint)diff.TotalSeconds;

            player.LastCheckStructureHut = DateTime.Now;

            player.Gold += goldGenerated;
        }

        #region ClientPacketHandlePurchaseItem
        private static void ClientPacketHandlePurchaseItem(RPacketPurchaseItem data, Peer peer) 
        {
            var itemType = (ItemType)data.ItemId;

            if (itemType == ItemType.Hut)
            {
                uint hutCost = 25;

                var player = Players.Find(x => x.Peer.ID == peer.ID);

                AddGoldGeneratedFromStructures(player);

                // Player can't afford this
                if (player.Gold < hutCost) 
                {
                    var packetDataNotEnoughGold = new WPacketPurchaseItem {
                        PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.NotEnoughGold,
                        ItemId = (ushort)ItemType.Hut,
                        Gold = player.Gold
                    };
                    var serverPacketNotEnoughGold = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataNotEnoughGold);
                    Send(serverPacketNotEnoughGold, peer, PacketFlags.Reliable);

                    return;
                }

                // Player bought the structure
                player.Gold -= hutCost;
                player.StructureHut++;

                Logger.Log($"Player '{player.Username}' purchased a Hut");

                var packetDataPurchasedItem = new WPacketPurchaseItem { 
                    PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.Purchased,
                    ItemId = (ushort)ItemType.Hut,
                    Gold = player.Gold
                };
                var serverPacketPurchasedItem = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataPurchasedItem);
                Send(serverPacketPurchasedItem, peer, PacketFlags.Reliable);
            }
        }
        #endregion
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
        ClearPlayerStats
    }
}
