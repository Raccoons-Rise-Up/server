using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class StructureHut : Structure
    {
        public StructureHut()
        {
            Cost = new()
            {
                { ResourceType.Wood, 100 },
                { ResourceType.Wheat, 23 }
            };
        }
    }
}
