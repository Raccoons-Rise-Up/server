using System;
using System.Collections.Generic;
using GameServer.Database;
using ENet;

namespace GameServer.Server
{
    public class Player : ModelPlayer
    {
        public Peer Peer { get; set; }
        public uint Id => Peer.ID;
        public string Ip => Peer.IP;
    }
}
