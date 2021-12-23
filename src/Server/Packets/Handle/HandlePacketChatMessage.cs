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

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketChatMessage();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            var player = ENetServer.Players[peer.ID];

            // Not sure if this is allowed with threading but going to try anyways!
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChatMessage, new WPacketChatMessage
            {
                Message = data.Message
            }), ENetServer.GetOtherPeers(peer));

            Logger.Log($"{player.Username}: {data.Message}");
        }
    }
}
