using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace GameServer.Server.Packets
{
    public class RPacketLogin : IReadable
    {
        public byte versionMajor;
        public byte versionMinor;
        public byte versionPatch;
        public string username;

        public void Read(PacketReader reader)
        {
            versionMajor = reader.ReadByte();
            versionMinor = reader.ReadByte();
            versionPatch = reader.ReadByte();
            username = reader.ReadString();
        }
    }
}
