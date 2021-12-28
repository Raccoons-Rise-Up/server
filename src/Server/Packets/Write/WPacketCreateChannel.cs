using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using System.Collections.Generic;

namespace GameServer.Server.Packets
{
    public class WPacketCreateChannel : IWritable
    {
        public ResponseChannelCreateOpcode ResponseChannelCreateOpcode { get; set; }
        public Dictionary<uint, string> Users { get; set; }
        public uint CreatorId { get; set; }
        public uint ChannelId { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)ResponseChannelCreateOpcode);

            writer.Write(ChannelId);

            if (ResponseChannelCreateOpcode == ResponseChannelCreateOpcode.Success) 
            {
                writer.Write(CreatorId);

                writer.Write((ushort)Users.Count);
                foreach (var pair in Users) 
                {
                    writer.Write((uint)pair.Key); // user id
                    writer.Write((string)pair.Value); // user username
                }
            }
        }
    }
}