using Common.Networking.IO;
using Common.Networking.Message;
using System.Collections.Generic;

namespace GameServer.Server.Packets
{
    public class WPacketCreateChannel : IWritable
    {
        public string ChannelName { get; set; }
        public uint OtherUserId { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(ChannelName);
            writer.Write(OtherUserId);
        }
    }
}