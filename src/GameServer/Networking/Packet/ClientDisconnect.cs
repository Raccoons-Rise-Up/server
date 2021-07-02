using System.Linq;

using ENet;

using GameServer.Networking.Message;
using Common.Networking.Packet;
using Common.Networking.Message;

namespace GameServer.Networking.Packet
{
    public class ClientDisconnect : HandlePacket
    {
        public override void Run(params object[] args)
        {
            var id = (uint)args[0];
            var netEvent = (Event)args[1];

            var peersToSend = Server.clients.FindAll(x => x.Status == ClientStatus.InGame && x.ID != id).Select(x => x.Peer).ToArray();
            var message = new MessageDisconnect(netEvent.Peer.ID);
            Network.Broadcast(ServerPacketType.ClientDisconnected, message, peersToSend);
            
            netEvent.Peer.Disconnect(netEvent.Peer.ID);
            //Console.Log($"Client '{netEvent.Peer.ID}' disconnected");
        }
    }
}