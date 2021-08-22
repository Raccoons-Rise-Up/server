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

        private static readonly List<Player> players = new();

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

                                var readBuffer = new byte[1024];
                                var readStream = new MemoryStream(readBuffer);
                                var reader = new BinaryReader(readStream);

                                readStream.Position = 0;
                                netEvent.Packet.CopyTo(readBuffer);

                                var opcode = (ClientPacketType)reader.ReadByte();

                                if (opcode == ClientPacketType.Login) 
                                {
                                    var data = new PacketLogin();
                                    var packetReader = new PacketReader(readBuffer);
                                    data.Read(packetReader);

                                    ClientPacketHandleLogin(data);
                                }

                                if (opcode == ClientPacketType.PurchaseItem) 
                                {
                                    var data = new PacketPurchaseItem();
                                    var packetReader = new PacketReader(readBuffer);
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

        private static void ClientPacketHandleLogin(PacketLogin data) 
        {
            // Check if versions match
            if (data.versionMajor != SERVER_VERSION_MAJOR || data.versionMinor != SERVER_VERSION_MINOR ||
                data.versionPatch != SERVER_VERSION_PATCH)
            {
                var clientVersion = $"{data.versionMajor}.{data.versionMinor}.{data.versionPatch}";
                var serverVersion = $"{SERVER_VERSION_MAJOR}.{SERVER_VERSION_MINOR}.{SERVER_VERSION_PATCH}";

                Logger.Log($"User '{data.username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");
                return;
            }

            // Check if username exists in database
            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();

            var player = dbPlayers.Find(x => x.Username == data.username);

            if (player != null)
            {
                // Add the player to the players list
                players.Add(new Player
                {
                    LastSeen = DateTime.Now
                });

                // Update the player in the database
                player.LastSeen = DateTime.Now;
                db.SaveChanges();

                Logger.Log($"User '{data.username}' logged in");
            }
            else
            {
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
        }

        private static void ClientPacketHandlePurchaseItem(PacketPurchaseItem data, Peer peer) 
        {
            if (data.itemId == 0)
            {
                using var db = new DatabaseContext();

                // Read
                Logger.Log("Query for player");
                // TODO: Find the appropriate player
                var player = db.Players.First();

                // Update
                Logger.Log("Updating the player");
                player.StructureHut++;

                db.SaveChanges();
            }


            var packetData = new PacketPurchasedItem((ushort)data.itemId);
            var serverPacket = new ServerPacket(ServerPacketType.PurchasedItem, packetData);

            Send(serverPacket, peer, PacketFlags.Reliable);
        }

        /// <summary>
        /// Send data to a specific peer
        /// </summary>
        /// <param name="gamePacket"></param>
        /// <param name="peer"></param>
        /// <param name="packetFlags"></param>
        private static void Send(GamePacket gamePacket, Peer peer, PacketFlags packetFlags)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, packetFlags);
            byte channelID = 0;
            peer.Send(channelID, ref packet);
        }
    }
}
