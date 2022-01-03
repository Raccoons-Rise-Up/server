using System.Collections.Generic;
using Common.Netcode;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketPlayerJoinLeave : IWritable
    {
        public JoinLeaveOpcode JoinLeaveOpcode { get; set; }
        public uint PlayerId { get; set; }
        public string PlayerName { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)JoinLeaveOpcode);
            writer.Write(PlayerId);

            if (JoinLeaveOpcode == JoinLeaveOpcode.Join)
                writer.Write(PlayerName);
        }
    }
}