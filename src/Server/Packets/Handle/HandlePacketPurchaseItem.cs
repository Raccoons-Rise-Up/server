using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Logging;

namespace GameServer.Server.Packets
{
    public class HandlePacketPurchaseItem : HandlePacket
    {
        public override ClientOpcode Opcode { get; set; }

        public HandlePacketPurchaseItem()
        {
            Opcode = ClientOpcode.PurchaseItem;
        }

        public override void Handle(Event netEvent, PacketReader packetReader)
        {
            var data = new RPacketPurchaseItem();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            var player = ENetServer.Players.Find(x => x.Peer.ID == peer.ID);
            // Add resources to player cache
            ENetServer.AddResourcesGeneratedFromStructures(player);

            var itemType = (ItemType)data.ItemId;

            if (itemType == ItemType.Hut)
            {
                uint hutCost = 25;
                

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
                player.StructureHuts++;

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

            packetReader.Dispose();
        }

        private static bool CanAfford(Player player, ItemType itemType) 
        {
            var canAfford = true;

            if (itemType == ItemType.Hut) 
            {
                foreach (var item in ENetServer.StructureHut.Cost) 
                {
                    if (item.Key == Resource.Wood) 
                        if (player.Wood < item.Value) 
                            canAfford = false;

                    if (item.Key == Resource.Stone)
                        if (player.Stone < item.Value)
                            canAfford = false;

                    if (item.Key == Resource.Gold)
                        if (player.Gold < item.Value)
                            canAfford = false;

                    if (item.Key == Resource.Wheat)
                        if (player.Wheat < item.Value)
                            canAfford = false;
                }
            }

            return canAfford;
        }
    }
}
