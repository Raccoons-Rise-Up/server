using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketChannelList : IWritable
    {
        public Dictionary<uint, Channel> Channels { get; set; }

        public void Write(PacketWriter writer) 
        {
            writer.Write((ushort)Channels.Count);
            foreach (var channelPair in Channels)
            {
                var channelId = channelPair.Key;
                var channel = channelPair.Value;

                writer.Write(channelId);
                writer.Write(channel.CreatorId);

                writer.Write((uint)channel.Messages.Count);
                foreach (var message in channel.Messages) 
                {
                    writer.Write((uint)message.UserId);
                    writer.Write((string)message.Message);
                }

                writer.Write((ushort)channel.Users.Count);
                foreach (var userPair in channel.Users)
                {
                    var userId = userPair.Key;
                    var user = userPair.Value;

                    writer.Write((uint)userId);
                    writer.Write((string)user.Username);
                }
            }
        }
    }
}
