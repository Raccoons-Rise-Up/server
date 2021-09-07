using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketPurchaseItem : IWritable
    {
        public PurchaseItemResponseOpcode PurchaseItemResponseOpcode { get; set; }
        public ushort StructureId { get; set; }
        public Dictionary<ResourceType, uint> Resources { get; set; }
        public byte ResourcesLength { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)PurchaseItemResponseOpcode);
            writer.Write(StructureId);
            writer.Write(ResourcesLength);
            foreach (var resource in Resources) 
            {
                writer.Write((byte)resource.Key);
                writer.Write(resource.Value);
            }
        }
    }
}
