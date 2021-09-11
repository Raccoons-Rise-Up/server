using System.Collections.Generic;

namespace GameServer.Server
{
    public abstract class ENetCmd
    {
        public abstract ServerOpcode Opcode { get; set; }

        public abstract void Handle(List<object> value);
    }
}
