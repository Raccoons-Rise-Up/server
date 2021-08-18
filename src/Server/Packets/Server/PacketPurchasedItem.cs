using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class PacketPurchasedItem : IWritable
    {
        private readonly ushort itemId;

        public PacketPurchasedItem(ushort m_ItemID)
        {
            this.itemId = m_ItemID;
        }

        public void Write(PacketWriter writer)
        {
            writer.Write(itemId);
        }
    }
}
