using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketChatMessage : IWritable
    {
        public uint ChannelId { get; set; }
        public uint UserId { get; set; }
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(ChannelId);
            writer.Write(UserId);
            writer.Write(Message);
        }
    }
}