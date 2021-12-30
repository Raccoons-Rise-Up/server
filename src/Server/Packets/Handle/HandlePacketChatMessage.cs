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
    public class HandlePacketChatMessage : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketChatMessage() => Opcode = ClientPacketOpcode.ChatMessage;

        private readonly string[] BlockedStrings = new string[] 
        {
            "fuck",
            "nigga",
            "nigger",
            "cunt",
            "bitch"
        };

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketChatMessage();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            var player = ENetServer.Players[peer.ID];

            var message = data.Message;

            // Remove any trailing spaces at start and end
            message = message.Trim();

            // Swear filter
            foreach (var blockedString in BlockedStrings)
                message = message.Replace(blockedString, "meowww");

            // Add message to channel
            ENetServer.Channels[data.ChannelId].Messages.Add(new UIMessage {
                UserId = peer.ID,
                Message = message
            });

            if (data.ChannelId == (uint)SpecialChannel.Global || data.ChannelId == (uint)SpecialChannel.Game)
            {
                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChatMessage, new WPacketChatMessage
                {
                    UserId = peer.ID,
                    ChannelId = data.ChannelId,
                    Message = message
                }), ENetServer.GetAllPeers());
            }
            else 
            {
                var peers = new List<Peer>();

                foreach (var userId in ENetServer.Channels[data.ChannelId].Users.Keys)
                    if (ENetServer.Players.ContainsKey(userId)) // Only send the chat message to users that are online
                        peers.Add(ENetServer.Players[userId].Peer);

                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChatMessage, new WPacketChatMessage
                {
                    UserId = peer.ID,
                    ChannelId = data.ChannelId,
                    Message = message
                }), peers);
            }

            Logger.Log($"[{data.ChannelId}] {player.Username}: {message}");
        }
    }
}
