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
        public const int SERVER_VERSION_MAJOR = 0;
        public const int SERVER_VERSION_MINOR = 1;
        public const int SERVER_VERSION_PATCH = 0;

        private const uint STARTING_GOLD = 100;
        private const uint STARTING_STRUCTURE_HUTS = 0;

        public static readonly ConcurrentQueue<ServerInstruction> serverInstructions = new();

        private const int PACKET_SIZE_MAX = 1024;

        private static readonly Dictionary<uint, Player> players = new();

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
                    while (serverInstructions.TryDequeue(out ServerInstruction result))
                    {
                        if (result.Opcode == ServerInstructionOpcode.ClearPlayerStats)
                        {
                            var player = players.Values.ToList().Find(x => x.Username == result.Data[0].ToString());
                            if (player != null) 
                            {
                                player.Gold = STARTING_GOLD;
                                player.StructureHut = STARTING_STRUCTURE_HUTS;
                                player.LastSeen = DateTime.Now;
                            }
                            
                            break;
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

                        switch (netEvent.Type)
                        {
                            case EventType.None:
                                break;

                            case EventType.Connect:
                                Logger.Log("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                break;

                            case EventType.Disconnect:
                                Logger.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                break;

                            case EventType.Timeout:
                                Logger.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                break;

                            case EventType.Receive:
                                //Logger.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                                var peer = netEvent.Peer;
                                var packet = netEvent.Packet;

                                var readBuffer = new byte[PACKET_SIZE_MAX];
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
                                break;
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
        #endregion

        #region ClientPacketHandleLogin
        private static void ClientPacketHandleLogin(RPacketLogin data, Peer peer) 
        {
            // Check if versions match
            if (data.VersionMajor != SERVER_VERSION_MAJOR || data.VersionMinor != SERVER_VERSION_MINOR ||
                data.VersionPatch != SERVER_VERSION_PATCH)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{SERVER_VERSION_MAJOR}.{SERVER_VERSION_MINOR}.{SERVER_VERSION_PATCH}";

                Logger.Log($"User '{data.Username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packetDataLoginVersionMismatch = new WPacketLogin 
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    VersionMajor = SERVER_VERSION_MAJOR,
                    VersionMinor = SERVER_VERSION_MINOR,
                    VersionPatch = SERVER_VERSION_PATCH,
                };

                Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetDataLoginVersionMismatch), peer, PacketFlags.Reliable);

                return;
            }

            // Check if username exists in database
            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();

            var dbPlayer = dbPlayers.Find(x => x.Username == data.Username);

            // These values will be sent to the client
            uint playerGold;
            uint playerStructureHuts;

            if (dbPlayer != null)
            {
                // RETURNING PLAYER

                playerGold = dbPlayer.Gold;
                playerStructureHuts = dbPlayer.StructureHut;

                // Add the player to the list of players currently on the server
                players.Add((uint)players.Count, new Player
                {
                    Peer = peer,
                    LastSeen = DateTime.Now,
                    Gold = dbPlayer.Gold
                });

                // Update the player in the database
                dbPlayer.LastSeen = DateTime.Now;
                db.SaveChanges();

                Logger.Log($"User '{data.Username}' logged in");
            }
            else
            {
                // NEW PLAYER

                playerGold = STARTING_GOLD;
                playerStructureHuts = STARTING_STRUCTURE_HUTS;

                // Add the player to the list of players currently on the server
                players.Add((uint)players.Count, new Player
                {
                    Peer = peer,
                    Username = data.Username,
                    Gold = STARTING_GOLD,
                    LastSeen = DateTime.Now
                });

                // Player does not exist in database, they are logging in for the first time
                db.Add(new ModelPlayer
                {
                    Username = data.Username,
                    Gold = STARTING_GOLD,
                    LastSeen = DateTime.Now
                });
                db.SaveChanges();

                Logger.Log($"User '{data.Username}' logged in for the first time");
            }

            var packetDataLoginSuccess = new WPacketLogin
            {
                LoginOpcode = LoginResponseOpcode.LoginSuccess,
                Gold = playerGold,
                StructureHuts = playerStructureHuts
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

                var player = players[peer.ID];

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

    public class ServerInstruction 
    {
        public ServerInstructionOpcode Opcode { get; set; }
        public List<object> Data { get; set; }

        public ServerInstruction(ServerInstructionOpcode opcode) 
        {
            Opcode = opcode;
            Data = new List<object>();
        }

        public void Write(object obj) 
        {
            Data.Add(obj);
        }
    }

    public enum ServerInstructionOpcode 
    {
        ClearPlayerStats
    }
}
