using Common.Networking.Message;
using Common.Networking.IO;

namespace GameServer.Networking.Message 
{
    public class MessagePositionUpdate : IWritable
    {
        private uint id;
        private float x, y;

        public MessagePositionUpdate(uint id, float x, float y) 
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }

        public void Write(PacketWriter writer) 
        {
            writer.Write(id);
            writer.Write(x);
            writer.Write(y);
        }
    }
}