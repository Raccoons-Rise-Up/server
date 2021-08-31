using Common.Networking.IO;
using ENet;

namespace GameServer.Server.Packets
{
    public abstract class HandlePacket
    {
        public abstract ClientOpcode Opcode { get; set; }

        public abstract void Handle(Event netEvent, PacketReader packetReader);
    }
}
