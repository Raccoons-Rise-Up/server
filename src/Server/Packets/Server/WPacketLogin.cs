using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketLogin : IWritable
    {
        public LoginOpcode LoginOpcode { private get; set; }
        public byte VersionMajor { private get; set; }
        public byte VersionMinor { private get; set; }
        public byte VersionPatch { private get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)LoginOpcode);

            switch (LoginOpcode) 
            {
                case LoginOpcode.VERSION_MISMATCH:
                    writer.Write(VersionMajor);
                    writer.Write(VersionMinor);
                    writer.Write(VersionPatch);
                    break;
                case LoginOpcode.LOGIN_SUCCESS:
                    break;
            }
        }
    }
}
