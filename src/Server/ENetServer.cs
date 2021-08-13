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

                                if (opcode == ClientPacketType.PurchaseItem) 
                                {
                                    var data = new PacketPurchaseItem();
                                    var packetReader = new PacketReader(readBuffer);
                                    data.Read(packetReader);

                                    if (data.m_ItemID == 0)
                                    {
                                        using var db = new DatabaseContext();

                                        // Create
                                        db.Add(new Player { Gold = 100 });
                                        db.SaveChanges();

                                        // Read
                                        Logger.Log("Query for player");
                                        var player = db.Players.First();

                                        // Update
                                        Logger.Log("Updating the player");
                                        player.StructureHut++;

                                        db.SaveChanges();

                                        // Delete
                                        Logger.Log("Delete the player");
                                        db.Remove(player);
                                        db.SaveChanges();
                                    }

                                    
                                    var packetData = new PacketPurchasedItem((ushort)data.m_ItemID);
                                    var serverPacket = new ServerPacket(ServerPacketType.PurchasedItem, packetData);

                                    Send(serverPacket, peer, PacketFlags.Reliable);
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
