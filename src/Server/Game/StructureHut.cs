using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Server
{
    public class StructureHut : Structure
    {
        public StructureHut()
        {
            Cost = new()
            {
                { "Wood", 100 },
                { "Wheat", 23 }
            };
        }
    }
}
