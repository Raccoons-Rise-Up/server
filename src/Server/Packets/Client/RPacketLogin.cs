using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace GameServer.Server.Packets
{
    public class RPacketLogin : IReadable
    {
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionPatch { get; set; }
        public string Username { get; set; }

        public void Read(PacketReader reader)
        {
            VersionMajor = reader.ReadByte();
            VersionMinor = reader.ReadByte();
            VersionPatch = reader.ReadByte();
            Username = reader.ReadString();
        }
    }
}
