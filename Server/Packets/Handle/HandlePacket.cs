using Common.Netcode;
using ENet;

namespace GameServer.Server.Packets
{
    public abstract class HandlePacket
    {
        public abstract ClientPacketOpcode Opcode { get; set; }

        public abstract void Handle(Peer peer, PacketReader packetReader);
    }
}
