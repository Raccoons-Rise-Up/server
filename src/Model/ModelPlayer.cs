using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    public class ModelPlayer
    {
        // Generic
        public int ModelPlayerId { get; set; }
        public string Ip { get; set; }
        public string Username { get; set; }
        public DateTime LastSeen { get; set; }

        // Resources
        public uint ResourceWood { get; set; }
        public uint ResourceStone { get; set; }
        public uint ResourceWheat { get; set; }
        public uint ResourceGold { get; set; }

        // Structures
        public uint StructureHut { get; set; }
        public uint StructureWheatFarm { get; set; }

        // Structure Last Checks
        public DateTime LastCheckStructureHut { get; set; }
        public DateTime LastCheckStructureWheatFarm { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}
