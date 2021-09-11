using System.Collections.Generic;
using GameServer.Utilities;

namespace GameServer.Server
{
    public class ENetCmdPardonPlayer : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ENetCmdPardonPlayer() 
        {
            Opcode = ServerOpcode.PardonPlayer;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();

            BanManager.PardonOfflinePlayer(username);
        }
    }
}
