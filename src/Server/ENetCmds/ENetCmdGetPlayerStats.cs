using System;
using System.Collections.Generic;
using GameServer.Logging;

namespace GameServer.Server
{
    public class ENetCmdGetPlayerStats : ENetCmd
    {
        public override ServerOpcode Opcode { get; set; }

        public ENetCmdGetPlayerStats()
        {
            Opcode = ServerOpcode.GetPlayerStats;
        }

        public override void Handle(List<object> value)
        {
            Player player = null;
            foreach (var p in ENetServer.Players.Values) 
            {
                if (p.Username == value[0].ToString()) 
                {
                    player = p;
                }
            }

            if (player == null)
                return;

            // Add resources to player cache
            //player.AddResourcesGeneratedFromStructures();

            var diff = DateTime.Now - player.LastSeen;
            var diffReadable = $"Days: {diff.Days}, Hours: {diff.Hours}, Minutes: {diff.Minutes}, Seconds: {diff.Seconds}";

            /*Logger.LogRaw(
                $"\n\nCACHE" +
                $"\nUsername: {player.Username} " +
                $"\nGold: {player.ResourceGold}" +
                $"\nStructure Huts: {player.StructureHut}" +
                $"\nLast Seen: {diffReadable}"
            );*/
        }
    }
}
