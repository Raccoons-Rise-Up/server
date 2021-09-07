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
        public uint Wood { get; set; }
        public uint Stone { get; set; }
        public uint Wheat { get; set; }
        public uint Gold { get; set; }

        // Structures
        public uint StructureHuts { get; set; }
        public DateTime LastCheckStructureHut { get; set; }
        public uint StructureWheatFarms { get; set; }
        public DateTime LastCheckStructureWheatFarm { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}
