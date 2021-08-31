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
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class ServerInstructionKickPlayer : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ServerInstructionKickPlayer()
        {
            Opcode = ServerOpcode.KickPlayer;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();
            var player = ENetServer.Players.Find(x => x.Username == username);
            if (player == null)
            {
                Logger.Log($"No player with the username '{username}' is online");
                return;
            }

            player.Peer.DisconnectNow((uint)DisconnectOpcode.Kicked);
            ENetServer.Players.Remove(player);
            Logger.Log($"Player '{player.Username}' was kicked");
        }
    }
}
