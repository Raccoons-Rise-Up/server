using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class StructureWheatFarm : Structure
    {
        public StructureWheatFarm()
        {
            Cost = new()
            {
                { ResourceType.Wood, 100 }
            };

            Production = new() 
            {
                { ResourceType.Wheat, 1 }
            };
        }
    }
}
