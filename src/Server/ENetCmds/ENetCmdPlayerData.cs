using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Networking.Packet;
using ENet;
using GameServer.Logging;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class ENetCmdPlayerData : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ENetCmdPlayerData() 
        {
            Opcode = ServerOpcode.SendPlayerData;
        }

        public override void Handle(List<object> value)
        {
            var username = value[0].ToString();

            // Check if player is currently online
            Player player = null;
            foreach (var p in ENetServer.Players.Values)
            {
                if (p.Username == username)
                    player = p;
            }

            // Send updated data to player client
            if (player != null)
            {
                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.PlayerData, new WPacketPlayerData
                {
                    ResourceCounts = player.ResourceCounts.ToDictionary(x => x.Key, x => (uint)x.Value),
                    StructureCounts = player.StructureCounts
                }), player.Peer);
            }

            Logger.Log($"Reset \'{username}\' values");
        }
    }
}
