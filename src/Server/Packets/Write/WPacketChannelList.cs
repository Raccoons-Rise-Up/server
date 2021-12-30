using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketChannelList : IWritable
    {
        public Dictionary<uint, UIChannel> Channels { get; set; }

        public void Write(PacketWriter writer) 
        {
            writer.Write((ushort)Channels.Count);
            foreach (var pair in Channels)
            {
                var channelId = pair.Key;
                var channel = pair.Value;

                writer.Write(channelId);
                writer.Write(channel.CreatorId);

                writer.Write((uint)channel.Messages.Count);
                foreach (var message in channel.Messages) 
                {
                    writer.Write((uint)message.UserId);
                    writer.Write((string)message.Message);
                }

                writer.Write((ushort)channel.Users.Count);
                foreach (var user in channel.Users)
                {
                    writer.Write((uint)user.Key);
                    writer.Write((string)user.Value);
                }
            }
        }
    }
}
