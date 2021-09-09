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
            Description = "A source of food for the cats.";
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
