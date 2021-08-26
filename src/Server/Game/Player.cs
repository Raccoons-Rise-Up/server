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
            Gold = StartingValues.Gold;
            StructureHut = StartingValues.StructureHuts;
            LastCheckStructureHut = DateTime.Now;
        }
    }
}
