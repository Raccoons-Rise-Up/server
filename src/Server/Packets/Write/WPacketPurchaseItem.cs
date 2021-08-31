using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketPurchaseItem : IWritable
    {
        public PurchaseItemResponseOpcode PurchaseItemResponseOpcode { get; set; }
        public ushort ItemId { get; set; }
        public uint Gold { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)PurchaseItemResponseOpcode);
            writer.Write(ItemId);
            writer.Write(Gold);
        }
    }
}
