using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using System.Collections.Generic;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketCreateChannel : IWritable
    {
        public ResponseChannelCreateOpcode ResponseChannelCreateOpcode { get; set; }
        public List<uint> Users { get; set; }
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
                foreach (var userId in Users) 
                {
                    var user = ENetServer.Players[userId];

                    writer.Write((uint)userId);
                    writer.Write((string)user.Username);
                    writer.Write((byte)user.Status);
                }
            }
        }
    }
}