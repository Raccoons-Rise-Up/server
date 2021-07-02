using System.Collections.Generic;
using ENet;
using GameServer.Networking.Message;
using Common.Networking.Packet;
using GameServer.Logging;

namespace GameServer.Networking.Packet
{
    public class ClientRequestPositions : HandlePacket
    {
        public override void Run(params object[] args)
        {
            uint id = (uint) args[0];

            var peers = Server.GetPeersInGame();
            if (peers.Length == 0)
                return;

            var client = Server.clients.Find(x => x.ID.Equals(id));
            if (!Server.positionPacketQueue.Contains(client))
            {
                Server.positionPacketQueue.Add(client);
                SendInitialPositions(client);

                //Console.Log($"Client {client.ID} requested initial positions, adding to queue..");
            }
        }

        private void SendInitialPositions(Client recipient)
        {
            var clientsInGame = Server.clients.FindAll(x => x.Status == ClientStatus.InGame && x.ID != recipient.ID);

            if (clientsInGame.Count < 1)
                return;

            foreach (var client in clientsInGame)
            {
                var data = new List<object>();

                data.Add(client.ID);
                data.Add(client.x);
                data.Add(client.y);

                var message = new MessagePositionUpdate(client.ID, client.x, client.y);
                var peers = new Peer[] { recipient.Peer};
                Network.Broadcast(ServerPacketType.PositionUpdates, message, peers, PacketFlags.None);

                Logger.Log("test");
            }
        }
    }
}