using GameServer.Console;
using System.Collections.Generic;

namespace GameServer.Server
{
    public class ENetCmdClearPlayerStats : ENetCmd
    {
        public override ENetOpcode Opcode { get; set; }

        public ENetCmdClearPlayerStats()
        {
            Opcode = ENetOpcode.ClearPlayerStats;
        }

        public override void Handle(List<object> value)
        {

            foreach (var player in ENetServer.Players.Values) 
            {
                if (player.Username == value[0].ToString()) 
                {
                    //player.ResetValues();

                    Logger.Log($"Cleared {player.Username} from list");
                }
            }
        }
    }
}

