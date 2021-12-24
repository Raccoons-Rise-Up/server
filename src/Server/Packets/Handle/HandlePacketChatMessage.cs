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

            // Not sure if this is allowed with threading but going to try anyways!
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChatMessage, new WPacketChatMessage
            {
                PlayerId = peer.ID,
                Message = message
            }), ENetServer.GetAllPeers());

            Logger.Log($"{player.Username}: {message}");
        }
    }
}
