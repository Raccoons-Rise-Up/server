using Common.Networking.Message;
using Common.Networking.IO;

namespace GameServer.Networking.Message 
{
    public class MessageHandshake : IWritable
    {
        public void Write(PacketWriter writer) 
        {
            writer.Write("Test");
        }
    }
}