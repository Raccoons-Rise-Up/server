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
                return;

            ENetServer.Channels.Add(data.ChannelName, new List<uint> { peer.ID, data.OtherUserId });

            // Tell the other user were opening a channel with them
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel
            {
                ChannelName = data.ChannelName,
                OtherUserId = peer.ID
            }), ENetServer.Players[data.OtherUserId].Peer );

            // Tell the creator were opening a channel with the other user
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.CreateChannel, new WPacketCreateChannel
            {
                ChannelName = data.ChannelName,
                OtherUserId = data.OtherUserId
            }), peer);

            Logger.Log($"{player.Username} created channel '{data.ChannelName}'");
        }
    }
}
