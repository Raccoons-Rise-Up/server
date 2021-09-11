using System.Collections.Generic;
using GameServer.Logging;

namespace GameServer.Server
{
    public class ENetCmdClearPlayerStats : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ENetCmdClearPlayerStats()
        {
            Opcode = ServerOpcode.ClearPlayerStats;
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

