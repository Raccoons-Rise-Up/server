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
    public class ServerInstructionPardonPlayer : ServerInstruction
    {
        public override ServerInstructionOpcode Opcode { get; set; }

        public ServerInstructionPardonPlayer() 
        {
            Opcode = ServerInstructionOpcode.PardonPlayer;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();

            Utils.PardonOfflinePlayer(username);
        }
    }
}
