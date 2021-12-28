using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class RPacketCreateChannel : IReadable
    {
        public uint OtherUserId { get; set; }

        public void Read(PacketReader reader)
        {
            OtherUserId = reader.ReadUInt32();
        }
    }
}