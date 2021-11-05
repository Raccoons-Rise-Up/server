using System.Collections.Generic;
using Common.Networking.Packet;
using GameServer.Logging;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class ENetCmdKickPlayer : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ENetCmdKickPlayer()
        {
            Opcode = ServerOpcode.KickPlayer;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();

            Player player = null;
            foreach (var p in ENetServer.Players.Values)
            {
                if (p.Username == username)
                {
                    player = p;
                }
            }

            if (player == null)
            {
                Logger.Log($"No player with the username '{username}' is online");
                return;
            }

            player.Peer.DisconnectNow((uint)DisconnectOpcode.Kicked);
            ENetServer.Players.Remove(player.Peer.ID);
            Logger.Log($"Player '{player.Username}' was kicked");
        }
    }
}
