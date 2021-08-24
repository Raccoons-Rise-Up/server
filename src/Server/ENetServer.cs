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

namespace GameServer.Server
{
    public class ENetServer
    {
        public const int serverVersionMajor = 0;
        public const int serverVersionMinor = 1;
        public const int serverVersionPatch = 0;

        public static readonly ConcurrentQueue<ServerInstructions> serverInstructions = new();
        public static readonly List<Player> players = new();

        private const int packetSizeMax = 1024;

        #region WorkerThread
        public static void WorkerThread() 
        {
            Thread.CurrentThread.Name = "SERVER";

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
                    while (serverInstructions.TryDequeue(out ServerInstructions result))
                    {
                        foreach (var cmd in result.Instructions) 
                        {
                            if (cmd.Key == ServerInstructionOpcode.ClearPlayerStats) 
                            {
                                var player = players.Find(x => x.Username == cmd.Value[0].ToString());
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

                        /*if (eventType == EventType.None) 
                        {
                            // Do nothing
                        }*/

                        if (eventType == EventType.Connect) 
                        {
                            //netEvent.Peer.Timeout(1000, 1000, 1000);
                        }

                        if (eventType == EventType.Disconnect) 
                        {
                            var player = RemovePlayer(netEvent.Peer.ID);
                            Logger.Log($"Player '{player.Username}' disconnected");
                        }

                        if (eventType == EventType.Timeout) 
                        {
                            var player = RemovePlayer(netEvent.Peer.ID);
                            Logger.Log($"Player '{player.Username}' timed out");
                        }

                        if (eventType == EventType.Receive) 
                        {
                            //Logger.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                            var peer = netEvent.Peer;
                            var packet = netEvent.Packet;

                            var readBuffer = new byte[packetSizeMax];
                            var packetReader = new PacketReader(readBuffer);
                            //packetReader.BaseStream.Position = 0;

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
                            packet.Dispose();
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

        private static Player RemovePlayer(uint id)
        {
            var player = players.Find(x => x.Peer.ID == id);

            // Save player to database
            using var db = new DatabaseContext();

            db.Add((ModelPlayer)player);
            db.SaveChanges();

            // Remove player from player list
            players.Remove(player);

            return player;
        }
        #endregion

        #region ClientPacketHandleLogin
        private static void ClientPacketHandleLogin(RPacketLogin data, Peer peer) 
        {
            // Check if versions match
            if (data.VersionMajor != serverVersionMajor || data.VersionMinor != serverVersionMinor ||
                data.VersionPatch != serverVersionPatch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{serverVersionMajor}.{serverVersionMinor}.{serverVersionPatch}";

                Logger.Log($"User '{data.Username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packetDataLoginVersionMismatch = new WPacketLogin 
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    VersionMajor = serverVersionMajor,
                    VersionMinor = serverVersionMinor,
                    VersionPatch = serverVersionPatch,
                };

                Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetDataLoginVersionMismatch), peer, PacketFlags.Reliable);

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
                var player = (Player)dbPlayer;
                player.LastSeen = DateTime.Now;
                players.Add(player);

                Logger.Log($"User '{data.Username}' logged in");
            }
            else
            {
                // NEW PLAYER

                playerValues.Gold = StartingValues.Gold;
                playerValues.StructureHuts = StartingValues.StructureHuts;

                // Add the player to the list of players currently on the server
                players.Add(new Player
                {
                    Peer = peer,
                    Username = data.Username,
                    Gold = StartingValues.Gold,
                    LastSeen = DateTime.Now
                });

                Logger.Log($"User '{data.Username}' logged in for the first time");
            }

            var packetDataLoginSuccess = new WPacketLogin
            {
                LoginOpcode = LoginResponseOpcode.LoginSuccess,
                Gold = playerValues.Gold,
                StructureHuts = playerValues.StructureHuts
            };

            Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetDataLoginSuccess), peer, PacketFlags.Reliable);
        }
        #endregion

        #region ClientPacketHandlePurchaseItem
        private static void ClientPacketHandlePurchaseItem(RPacketPurchaseItem data, Peer peer) 
        {
            var itemType = (ItemType)data.ItemId;

            if (itemType == ItemType.Hut)
            {
                uint hutCost = 25;

                var player = players.Find(x => x.Peer.ID == peer.ID);

                // Calculate players new gold value based on how many structures they own
                var diff = DateTime.Now - player.LastCheckStructureHut;
                uint goldGenerated = player.StructureHut * (uint)diff.TotalSeconds;

                player.Gold += goldGenerated;
                Logger.Log($"Huts: {player.StructureHut}, Gold generated: {goldGenerated}");

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
                player.LastCheckStructureHut = DateTime.Now;

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
        ClearPlayerStats
    }
}
