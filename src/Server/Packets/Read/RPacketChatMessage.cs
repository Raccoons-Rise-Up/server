using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class RPacketChatMessage : IReadable
    {
        public string Message { get; set; }

        public void Read(PacketReader reader)
        {
            Message = reader.ReadString();
        }
    }
}