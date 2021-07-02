using ENet;

using Common.Networking.Message;
using Common.Networking.Packet;

namespace GameServer.Networking
{
    class Network
    {
        public static void Send(ref Event netEvent, ENet.Packet packet)
        {
            netEvent.Peer.Send(Server.ChannelID, ref packet);
        }

        /*public static void Broadcast(ENet.Packet packet)
        {
            Server.Host.Broadcast(Server.ChannelID, ref packet);
        }

        public static void Broadcast(ENet.Packet packet, Peer excludedPeer)
        {
            Server.Host.Broadcast(Server.ChannelID, ref packet, excludedPeer);
        }*/

        public static void Broadcast(ServerPacketType serverPacketType, IWritable message, Peer[] peers, PacketFlags packetFlags = PacketFlags.Reliable)
        {
            var serverPacket = new ServerPacket(serverPacketType, message);
            var packet = default(ENet.Packet);
            packet.Create(serverPacket.Data, packetFlags);
            Server.Host.Broadcast(Server.ChannelID, ref packet, peers);
        }
    }
}