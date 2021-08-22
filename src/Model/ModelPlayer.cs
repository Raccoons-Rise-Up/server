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
        public int Gold { get; set; }
        public int StructureHut { get; set; }
    }
}
