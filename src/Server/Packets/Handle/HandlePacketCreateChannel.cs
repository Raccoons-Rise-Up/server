using System;
using System.Linq;
using System.Collections.Generic;
using Common.Networking.Packet;
using Common.Networking.IO;
using Common.Game;
using ENet;
using GameServer.Logging;

namespace GameServer.Server.Packets
{
    public class HandlePacketCreateChannel : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketCreateChannel() => Opcode = ClientPacketOpcode.CreateChannel;

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketCreateChannel();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            var creator = ENetServer.Players[peer.ID];
            var otherUser = ENetServer.Players[data.OtherUserId];

            // Check to see if this channel exists already
            foreach (var pair in ENetServer.Channels) 
            {
                var channel = pair.Value;

                // Do not check the special channels
                if (pair.Key == (uint)SpecialChannel.Global || pair.Key == (uint)SpecialChannel.Game)
                    continue;

                if (channel.Users.Count == 2 && channel.Users.Contains(peer.ID) && channel.Users.Contains(data.OtherUserId)) 
                {
                    Logger.Log($"{creator.Username} tried to create a channel but one exists already with users: {creator.Username}, {ENetServer.Players[data.OtherUserId].Username}");

                    ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel { 
                        ResponseChannelCreateOpcode = ResponseChannelCreateOpcode.ChannelExistsAlready,
                        ChannelId = pair.Key
                    }), creator.Peer);
                    return;
                }
            }

            var channelId = ENetServer.ChannelId++;
            var creatorId = creator.Peer.ID;
            var users = new List<uint>() {
                { creatorId         },
                { otherUser.Peer.ID }
            };

            // Create the channel
            ENetServer.Channels.Add(channelId, new Channel {
                CreatorId = creatorId,
                Users = users
            });

            // Tell the creator and the other user a new channel has been created
            Logger.Log($"Telling the creator '{creator.Username}' and other user '{otherUser.Username}' were opening a channel with them");
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel
            {
                ResponseChannelCreateOpcode = ResponseChannelCreateOpcode.Success,
                ChannelId = channelId,
                CreatorId = creatorId,
                Users = users
            }), new List<Peer> {
                creator.Peer,
                otherUser.Peer
            });
        }
    }
}
