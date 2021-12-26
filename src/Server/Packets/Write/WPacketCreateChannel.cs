using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using System.Collections.Generic;

namespace GameServer.Server.Packets
{
    public class WPacketCreateChannel : IWritable
    {
        public ResponseChannelCreateOpcode ResponseChannelCreateOpcode { get; set; }
        public string ChannelName { get; set; }
        public uint OtherUserId { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)ResponseChannelCreateOpcode);

            if (ResponseChannelCreateOpcode == ResponseChannelCreateOpcode.Success)
            {
                writer.Write(ChannelName);
                writer.Write(OtherUserId);
            }

            if (ResponseChannelCreateOpcode == ResponseChannelCreateOpcode.ChannelExistsAlready) 
            {
                writer.Write(ChannelName);
            }
        }
    }
}