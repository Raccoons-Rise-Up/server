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
    public static class HandlePacket
    {
        public static void Handle(ref Event netEvent)
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
            if (data.VersionMajor != ENetServer.ServerVersionMajor || data.VersionMinor != ENetServer.ServerVersionMinor ||
                data.VersionPatch != ENetServer.ServerVersionPatch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ENetServer.ServerVersionMajor}.{ENetServer.ServerVersionMinor}.{ENetServer.ServerVersionPatch}";

                Logger.Log($"Player '{data.Username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    VersionMajor = ENetServer.ServerVersionMajor,
                    VersionMinor = ENetServer.ServerVersionMinor,
                    VersionPatch = ENetServer.ServerVersionPatch,
                };

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData);
                ENetServer.Send(packet, peer, PacketFlags.Reliable);

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
                    Peer = peer,
                    Ip = peer.IP
                };

                ENetServer.Players.Add(player);

                Logger.Log($"Player '{data.Username}' logged in");
            }
            else
            {
                // NEW PLAYER

                playerValues.Gold = StartingValues.Gold;
                playerValues.StructureHuts = StartingValues.StructureHuts;

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(new Player
                {
                    Peer = peer,
                    Username = data.Username,
                    Gold = StartingValues.Gold,
                    LastSeen = DateTime.Now,
                    Ip = peer.IP
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
                ENetServer.Send(packet, peer, PacketFlags.Reliable);
            }
        }
        #endregion

        #region ClientPacketHandlePurchaseItem
        private static void ClientPacketHandlePurchaseItem(RPacketPurchaseItem data, Peer peer)
        {
            var itemType = (ItemType)data.ItemId;

            if (itemType == ItemType.Hut)
            {
                uint hutCost = 25;

                var player = ENetServer.Players.Find(x => x.Peer.ID == peer.ID);

                ENetServer.AddGoldGeneratedFromStructures(player);

                // Player can't afford this
                if (player.Gold < hutCost)
                {
                    var packetDataNotEnoughGold = new WPacketPurchaseItem
                    {
                        PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.NotEnoughGold,
                        ItemId = (ushort)ItemType.Hut,
                        Gold = player.Gold
                    };
                    var serverPacketNotEnoughGold = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataNotEnoughGold);
                    ENetServer.Send(serverPacketNotEnoughGold, peer, PacketFlags.Reliable);

                    return;
                }

                // Player bought the structure
                player.Gold -= hutCost;
                player.StructureHut++;

                Logger.Log($"Player '{player.Username}' purchased a Hut");

                var packetDataPurchasedItem = new WPacketPurchaseItem
                {
                    PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.Purchased,
                    ItemId = (ushort)ItemType.Hut,
                    Gold = player.Gold
                };
                var serverPacketPurchasedItem = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataPurchasedItem);
                ENetServer.Send(serverPacketPurchasedItem, peer, PacketFlags.Reliable);
            }
        }
        #endregion
    }
}
