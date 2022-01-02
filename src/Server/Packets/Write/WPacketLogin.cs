using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketLogin : IWritable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public ServerVersion ServerVersion { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)LoginOpcode);

            if (LoginOpcode == LoginResponseOpcode.VersionMismatch) 
            {
                writer.Write((byte)ServerVersion.Major);
                writer.Write((byte)ServerVersion.Minor);
                writer.Write((byte)ServerVersion.Patch);
                return;
            }
        }
    }
}
