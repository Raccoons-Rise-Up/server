using System;
using System.Collections.Generic;
using GameServer.Database;
using ENet;

namespace GameServer.Server
{
    public class Player : ModelPlayer
    {
        public Peer Peer { get; set; }

        public void ResetValues() 
        {
            LastSeen = DateTime.Now;
            Wood = 0;
            Wheat = 0;
            Stone = 0;
            Gold = 0;
            StructureHuts = 0;
            LastCheckStructureHut = DateTime.Now;
            StructureWheatFarms = 0;
            LastCheckStructureWheatFarm = DateTime.Now;
        }
    }
}
