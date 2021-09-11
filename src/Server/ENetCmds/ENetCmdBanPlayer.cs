using System.Collections.Generic;
using GameServer.Utilities;

namespace GameServer.Server
{
    public class ENetCmdBanPlayer : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ENetCmdBanPlayer() 
        {
            Opcode = ServerOpcode.BanPlayer;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();

            var bannedOnline = BanManager.BanOnlinePlayer(username);

            if (!bannedOnline)
                BanManager.BanOfflinePlayer(username);
        }
    }
}
