using Common.Networking.Message;
using Common.Networking.IO;

namespace GameServer.Networking.Message 
{
    public class MessageDisconnect : IWritable
    {
        private uint id;

        public MessageDisconnect(uint id) 
        {
            this.id = id;
        }

        public void Write(PacketWriter writer) 
        {
            writer.Write("Test");
        }
    }
}