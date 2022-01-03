using System.Collections.Generic;
using Common.Netcode;
using GameServer.Console;
using GameServer.Server.Game;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class ENetCmdKickPlayer : ENetCmd
    {
        public override ENetOpcode Opcode { get; set; }

        public ENetCmdKickPlayer()
        {
            Opcode = ENetOpcode.KickPlayer;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();

            ServerPlayer player = null;
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
