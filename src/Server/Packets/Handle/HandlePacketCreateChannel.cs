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

            var player = ENetServer.Players[peer.ID];

            if (ENetServer.Channels.ContainsKey(data.ChannelName)) 
            {
                Logger.Log($"Channel '{data.ChannelName}' exists server-side already");
                return;
            }

            // Search for channel duos with the same users
            foreach (var channel in ENetServer.Channels.Values) 
            {
                if (channel.Users.Count == 2 && channel.Users.Contains(peer.ID) && channel.Users.Contains(data.OtherUserId)) 
                {
                    Logger.Log($"{player.Username} tried to create a channel but one exists already with users: {player.Username}, {ENetServer.Players[data.OtherUserId].Username}");

                    ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel { 
                        ResponseChannelCreateOpcode = ResponseChannelCreateOpcode.ChannelExistsAlready,
                        ChannelName = data.ChannelName
                    }), peer);
                    return;
                }
            }

            ENetServer.Channels.Add(data.ChannelName, new UIChannel { 
                Name = ENetServer.Players[netEvent.Peer.ID].Username, // Set the channel name to the name of the creator
                Creator = netEvent.Peer.ID,
                Users = new List<uint> { peer.ID, data.OtherUserId }
            });

            // Tell the other user were opening a channel with them
            Logger.Log($"Telling the other user '{ENetServer.Players[data.OtherUserId].Username}' were opening a channel with them");
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel
            {
                ResponseChannelCreateOpcode = ResponseChannelCreateOpcode.Success,
                ChannelName = data.ChannelName,
                OtherUserId = peer.ID
            }), ENetServer.Players[data.OtherUserId].Peer );

            // Tell the creator were opening a channel with the other user
            Logger.Log($"Telling the creator '{player.Username}' were opening a channel with the other user '{ENetServer.Players[data.OtherUserId].Username}'");
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel
            {
                ChannelName = data.ChannelName,
                OtherUserId = data.OtherUserId
            }), peer);

            Logger.Log($"{player.Username} created channel '{data.ChannelName}'");
        }
    }
}
