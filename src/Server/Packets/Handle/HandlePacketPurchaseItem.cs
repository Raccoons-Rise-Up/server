using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Logging;
using GameServer.Server.Packets;

namespace GameServer.Server.Packets
{
    public class HandlePacketPurchaseItem : HandlePacket
    {
        public override ClientOpcode Opcode { get; set; }

        public HandlePacketPurchaseItem()
        {
            Opcode = ClientOpcode.PurchaseItem;
        }

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketPurchaseItem();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            var player = ENetServer.Players.Find(x => x.Peer.ID == peer.ID);

            var purchaseResult = player.TryPurchase(ENetServer.Structures[data.StructureId]);

            // Player can't afford this
            if (purchaseResult.Result == PurchaseEnumResult.LackingResources)
            {
                Logger.Log($"Player '{player.Username}' could not afford Hut");

                var packetDataNotEnoughGold = new WPacketPurchaseItem
                {
                    PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.NotEnoughGold,
                    StructureId = (ushort)StructureType.Hut,
                    Resources = purchaseResult.Resources
                };
                var serverPacketNotEnoughGold = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataNotEnoughGold);
                ENetServer.Send(serverPacketNotEnoughGold, peer, PacketFlags.Reliable);
                return;
            }

            // Player bought the structure
            if (purchaseResult.Result == PurchaseEnumResult.Success)
            {
                Logger.Log($"Player '{player.Username}' purchased a Hut");

                var packetDataPurchasedItem = new WPacketPurchaseItem
                {
                    PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.Purchased,
                    StructureId = (ushort)StructureType.Hut,
                    ResourcesLength = (byte)purchaseResult.Resources.Count,
                    Resources = purchaseResult.Resources
                };
                var serverPacketPurchasedItem = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataPurchasedItem);
                ENetServer.Send(serverPacketPurchasedItem, peer, PacketFlags.Reliable);
            }
        }
    }
}
