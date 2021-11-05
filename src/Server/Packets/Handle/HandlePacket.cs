using Common.Networking.IO;
using Common.Networking.Packet;
using ENet;

namespace GameServer.Server.Packets
{
    public abstract class HandlePacket
    {
        public abstract ClientPacketOpcode Opcode { get; set; }

        public abstract void Handle(Event netEvent, ref PacketReader packetReader);
    }
}
