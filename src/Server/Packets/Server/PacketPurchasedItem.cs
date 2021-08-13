using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class PacketPurchasedItem : IWritable
    {
        private readonly ushort m_ItemID;

        public PacketPurchasedItem(ushort m_ItemID)
        {
            this.m_ItemID = m_ItemID;
        }

        public void Write(PacketWriter writer)
        {
            writer.Write(m_ItemID);
        }
    }
}
