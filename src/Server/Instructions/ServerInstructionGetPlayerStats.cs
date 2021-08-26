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
    public class ServerInstructionGetPlayerStats : ServerInstruction
    {
        public override ServerInstructionOpcode Opcode { get; set; }

        public ServerInstructionGetPlayerStats()
        {
            Opcode = ServerInstructionOpcode.GetPlayerStats;
        }

        public override void Handle(List<object> value)
        {
            var player = ENetServer.Players.Find(x => x.Username == value[0].ToString());
            if (player == null)
                return;

            ENetServer.AddGoldGeneratedFromStructures(player);

            var diff = DateTime.Now - player.LastSeen;
            var diffReadable = $"Days: {diff.Days}, Hours: {diff.Hours}, Minutes: {diff.Minutes}, Seconds: {diff.Seconds}";

            Logger.LogRaw(
                $"\n\nCACHE" +
                $"\nUsername: {player.Username} " +
                $"\nGold: {player.Gold}" +
                $"\nStructure Huts: {player.StructureHut}" +
                $"\nLast Seen: {diffReadable}"
            );
        }
    }
}
