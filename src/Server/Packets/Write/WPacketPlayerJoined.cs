using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketPlayerJoined : IWritable
    {
        public uint PlayerId { get; set; }
        public string PlayerName { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PlayerName);
        }
    }
}
