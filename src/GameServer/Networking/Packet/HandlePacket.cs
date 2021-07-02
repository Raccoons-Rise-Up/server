namespace GameServer.Networking.Packet
{
    public abstract class HandlePacket
    {
        public virtual void Run(params object[] args) {}
    }
}