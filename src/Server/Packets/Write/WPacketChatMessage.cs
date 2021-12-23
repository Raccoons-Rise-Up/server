using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketChatMessage : IWritable
    {
        public uint PlayerId { get; set; }
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(Message);
        }
    }
}