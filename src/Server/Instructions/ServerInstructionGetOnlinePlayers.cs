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
    public class ServerInstructionGetOnlinePlayers : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ServerInstructionGetOnlinePlayers()
        {
            Opcode = ServerOpcode.GetOnlinePlayers;
        }

        public override void Handle(List<object> value)
        {
            if (ENetServer.Players.Count == 0)
            {
                Logger.Log("There are 0 players on the server");
                return;
            }

            Logger.LogRaw($"\nOnline Players: {string.Join(' ', ENetServer.Players)}");
        }
    }
}
