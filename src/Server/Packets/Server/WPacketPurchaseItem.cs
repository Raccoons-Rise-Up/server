using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketPurchaseItem : IWritable
    {
        private readonly ushort itemId;

        public WPacketPurchaseItem(ushort m_ItemID)
        {
            this.itemId = m_ItemID;
        }

        public void Write(PacketWriter writer)
        {
            writer.Write(itemId);
        }
    }
}
