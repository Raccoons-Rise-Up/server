using System;
using System.Linq;
using System.Collections.Generic;
using Common.Networking.Packet;
using Common.Networking.IO;
using Common.Game;
using ENet;
using GameServer.Logging;

namespace GameServer.Server.Packets
{
    public class HandlePacketPurchaseItem : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketPurchaseItem()
        {
            Opcode = ClientPacketOpcode.PurchaseItem;
        }

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketPurchaseItem();
            data.Read(packetReader);
            
            var peer = netEvent.Peer;

            var player = ENetServer.Players[peer.ID];
            var structure = ENetServer.StructureInfoData[(StructureType)data.StructureId];

            var purchaseResult = player.TryPurchase(structure);

            // Player can't afford this
            if (purchaseResult.Result == PurchaseEnumResult.LackingResources)
            {
                Logger.Log($"Player '{player.Username}' could not afford '1 x {structure.Name}'");

                // TODO: Change from "not enough gold" to "not enough resources"
                var packetDataNotEnoughGold = new WPacketPurchaseItem
                {
                    PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.NotEnoughGold,
                    ResourcesLength = (byte)purchaseResult.Resources.Count,
                    Resources = purchaseResult.Resources
                };
                var serverPacketNotEnoughGold = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataNotEnoughGold);
                ENetServer.Send(serverPacketNotEnoughGold, peer, PacketFlags.Reliable);
                return;
            }

            // Player bought the structure
            if (purchaseResult.Result == PurchaseEnumResult.Success)
            {
                Logger.Log($"Player '{player.Username}' purchased '1 x {structure.Name}'");

                var packetDataPurchasedItem = new WPacketPurchaseItem
                {
                    PurchaseItemResponseOpcode = PurchaseItemResponseOpcode.Purchased,
                    ResourcesLength = (byte)purchaseResult.Resources.Count,
                    Resources = purchaseResult.Resources
                };
                var serverPacketPurchasedItem = new ServerPacket((byte)ServerPacketOpcode.PurchasedItem, packetDataPurchasedItem);
                ENetServer.Send(serverPacketPurchasedItem, peer, PacketFlags.Reliable);
            }
        }
    }
}
