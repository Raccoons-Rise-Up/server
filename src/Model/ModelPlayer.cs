using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    public class ModelPlayer
    {
        public int ModelPlayerId { get; set; }
        public string Username { get; set; }
        public DateTime LastSeen { get; set; }
        public uint Gold { get; set; }
        public uint StructureHut { get; set; }
        public DateTime LastCheckStructureHut { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}
