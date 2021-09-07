using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Server
{
    public class StructureWheatFarm : Structure
    {
        public StructureWheatFarm()
        {
            Cost = new()
            {
                { Resource.Wood, 100 }
            };

            Production = new() 
            {
                { Resource.Wheat, 1 }
            };
        }
    }
}
