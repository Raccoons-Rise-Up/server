using System;
using System.Collections.Generic;
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

                                var opcode = (ClientPacketType)packetReader.ReadByte();

                                if (opcode == ClientPacketType.Login) 
                                {
                                    var data = new RPacketLogin();
                                    data.Read(packetReader);

                                    ClientPacketHandleLogin(data, peer);
                                }

                                if (opcode == ClientPacketType.PurchaseItem) 
                                {
                                    var data = new RPacketPurchaseItem();
                                    data.Read(packetReader);

                                    ClientPacketHandlePurchaseItem(data, peer);
                                }

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
            if (data.versionMajor != SERVER_VERSION_MAJOR || data.versionMinor != SERVER_VERSION_MINOR ||
                data.versionPatch != SERVER_VERSION_PATCH)
            {
                var clientVersion = $"{data.versionMajor}.{data.versionMinor}.{data.versionPatch}";
                var serverVersion = $"{SERVER_VERSION_MAJOR}.{SERVER_VERSION_MINOR}.{SERVER_VERSION_PATCH}";

                Logger.Log($"User '{data.username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packetDataLoginVersionMismatch = new WPacketLogin 
                {
                    LoginOpcode = LoginOpcode.VERSION_MISMATCH,
                    VersionMajor = SERVER_VERSION_MAJOR,
                    VersionMinor = SERVER_VERSION_MINOR,
                    VersionPatch = SERVER_VERSION_PATCH
                };

                Send(new ServerPacket((byte)ServerPacketType.LoginResponse, packetDataLoginVersionMismatch), peer, PacketFlags.Reliable);

                return;
            }

            // Check if username exists in database
            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();

            var player = dbPlayers.Find(x => x.Username == data.username);

            if (player != null)
            {
                // Player exists in the database

                // Add the player to the list of players currently on the server
                players.Add((uint)players.Count, new Player
                {
                    Peer = peer,
                    LastSeen = DateTime.Now
                });

                // Update the player in the database
                player.LastSeen = DateTime.Now;
                db.SaveChanges();

                Logger.Log($"User '{data.username}' logged in");
            }
            else
            {
                // Add the player to the list of players currently on the server
                players.Add((uint)players.Count, new Player
                {
                    Peer = peer,
                    Username = data.username,
                    Gold = 100,
                    LastSeen = DateTime.Now
                });

                // Player does not exist in database, they are logging in for the first time
                db.Add(new ModelPlayer
                {
                    Username = data.username,
                    Gold = 100,
                    LastSeen = DateTime.Now
                });
                db.SaveChanges();

                Logger.Log($"User '{data.username}' logged in for the first time");
            }

            var packetDataLoginSuccess = new WPacketLogin
            {
                LoginOpcode = LoginOpcode.LOGIN_SUCCESS
            };

            Send(new ServerPacket((byte)ServerPacketType.LoginResponse, packetDataLoginSuccess), peer, PacketFlags.Reliable);
        }
        #endregion

        #region ClientPacketHandlePurchaseItem
        private static void ClientPacketHandlePurchaseItem(RPacketPurchaseItem data, Peer peer) 
        {
            if (data.itemId == 0)
            {
                using var db = new DatabaseContext();

                var dbPlayers = db.Players.ToList();
                //var player = dbPlayers.Find(x => x.Username == data.username);

                // Read
                Logger.Log("Query for player");
                // TODO: Find the appropriate player
                var player = db.Players.First();

                // Update
                Logger.Log("Updating the player");
                player.StructureHut++;

                db.SaveChanges();
            }


            var packetData = new WPacketPurchaseItem((ushort)data.itemId);
            var serverPacket = new ServerPacket((byte)ServerPacketType.PurchasedItem, packetData);

            Send(serverPacket, peer, PacketFlags.Reliable);
        }
        #endregion
    }
}
