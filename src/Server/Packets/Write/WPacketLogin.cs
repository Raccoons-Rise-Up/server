using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketLogin : IWritable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionPatch { get; set; }
        public uint Wood { get; set; }
        public uint Stone { get; set; }
        public uint Wheat { get; set; }
        public uint Gold { get; set; }
        public uint StructureHuts { get; set; }
        public uint StructureWheatFarms { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)LoginOpcode);

            switch (LoginOpcode) 
            {
                case LoginResponseOpcode.VersionMismatch:
                    writer.Write(VersionMajor);
                    writer.Write(VersionMinor);
                    writer.Write(VersionPatch);
                    break;
                case LoginResponseOpcode.LoginSuccessReturningPlayer:
                    writer.Write(Wood);
                    writer.Write(Stone);
                    writer.Write(Wheat);
                    writer.Write(Gold);
                    writer.Write(StructureHuts);
                    writer.Write(StructureWheatFarms);
                    break;
            }
        }
    }
}
