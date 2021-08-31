using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Database;
using GameServer.Logging;
using GameServer.Utilities;

namespace GameServer.Server
{
    public class ServerInstructionClearPlayerStats : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ServerInstructionClearPlayerStats()
        {
            Opcode = ServerOpcode.ClearPlayerStats;
        }

        public override void Handle(List<object> value)
        {
            var player = ENetServer.Players.Find(x => x.Username == value[0].ToString());
            if (player != null)
            {
                player.ResetValues();

                Logger.Log($"Cleared {player.Username} from list");
            }
        }
    }
}

