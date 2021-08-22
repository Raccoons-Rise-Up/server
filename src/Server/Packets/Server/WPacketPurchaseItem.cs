using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketPurchaseItem : IWritable
    {
        public ushort ItemId { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(ItemId);
        }
    }
}
