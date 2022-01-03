using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ENet;

namespace Common.Game
{
    public class Player
    {
        [JsonIgnore] public Peer Peer { get; set; }
        public string Username { get; set; }
        public string Ip { get; set; }

        public Player(Peer peer, string username) 
        {
            Peer = peer;
            if (peer.IsSet)
                Ip = peer.IP;

            Username = username;
        }

        public override string ToString() => Username;
    }
}
