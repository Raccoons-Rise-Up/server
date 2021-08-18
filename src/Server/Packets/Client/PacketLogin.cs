using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace GameServer.Server.Packets
{
    public class PacketLogin : IReadable
    {
        public ClientPacketType id;
        public string username;

        public void Read(PacketReader reader)
        {
            id = (ClientPacketType)reader.ReadByte();
            username = reader.ReadString();
        }
    }
}
