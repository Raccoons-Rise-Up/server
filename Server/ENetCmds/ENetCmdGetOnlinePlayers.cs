using GameServer.Console;
using System.Collections.Generic;

namespace GameServer.Server
{
    public class ENetCmdGetOnlinePlayers : ENetCmd
    {
        public override ENetOpcode Opcode { get; set; }

        public ENetCmdGetOnlinePlayers()
        {
            Opcode = ENetOpcode.GetOnlinePlayers;
        }

        public override void Handle(List<object> value)
        {
            if (ENetServer.Players.Count == 0)
            {
                Logger.Log("There are 0 players on the server");
                return;
            }

            var players = new List<string>();
            foreach (var p in ENetServer.Players)
                players.Add($"{p.Value} ({p.Key})");

            Logger.LogRaw($"\nOnline Players: {string.Join(' ', players)}");
        }
    }
}
