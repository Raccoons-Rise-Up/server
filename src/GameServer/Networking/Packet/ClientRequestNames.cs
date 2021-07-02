using System;
using System.Collections.Generic;

using ENet;

namespace GameServer.Networking.Packet
{
    public class ClientRequestNames : HandlePacket
    {
        public override void Run(params object[] args)
        {
            uint id = (uint) args[0];
            var peers = Server.GetPeersInGame();
            if (peers.Length == 0)
                return;

            var client = Server.clients.Find(x => x.ID.Equals(id));
            SendName(client);
        }

        private void SendName(Client sender) 
        {
            var clientsInGame = Server.clients.FindAll(x => x.Status == ClientStatus.InGame && x.ID != sender.ID);

            if (clientsInGame.Count < 1)
                return;

            foreach (var client in clientsInGame) 
            {
                var data = new List<object>();

                data.Add(client.ID);
                data.Add(client.Name);

                //Network.Broadcast(Server.server, GamePacket.Create(ServerPacketType.ClientName, PacketFlags.Reliable, data.ToArray()), new Peer[] { client.Peer });
            }
        }
    }
}