using System.IO;

namespace GameServer.Networking.Packet
{
    public class ClientPositionUpdate : HandlePacket
    {
        public override void Run(params object[] args)
        {
            var id = (uint) args[0];
            var reader = (BinaryReader) args[1];

            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            //Console.Log($"Recieved x {x}, y {y}");

            var client = Server.clients.Find(x => x.ID.Equals(id));
            client.x = x;
            client.y = y;

            // Client will be added to the position update queue
            if (!Server.positionPacketQueue.Contains(client) && (client.x != client.px || client.y != client.py))
            {
                Server.positionPacketQueue.Add(client);
            }

            // Keep track of previous position
            client.px = client.x;
            client.py = client.y;

            //Console.Log(client);
        }
    }
}