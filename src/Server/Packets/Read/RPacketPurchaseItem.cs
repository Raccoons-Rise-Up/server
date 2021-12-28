using Common.Networking.Message;
using Common.Networking.IO;

namespace GameServer.Server.Packets
{
    public class RPacketPurchaseItem : IReadable
    {
        public ushort StructureId { get; set; }

        public void Read(PacketReader reader)
        {
            StructureId = reader.ReadUInt16();
        }
    }
}
