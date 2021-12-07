using System;
using System.Diagnostics;
using System.Linq;
using GameServer.Server;
using GameServer.Utilities;

namespace GameServer.Logging.Commands
{
    public class CommandWhoIs : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandWhoIs() 
        {
            Description = "View more information about a specific player";
            Usage = "<player>";
            Aliases = new string[] { "who" };
        }

        public override void Run(string[] args) 
        {
            if (args.Length == 0) 
            {
                Logger.Log("Please provide a username to search for");
                return;
            }

            var playerSearchResultData = ServerUtils.FindPlayer(args[0]);

            if (playerSearchResultData.SearchResult == ServerUtils.PlayerSearchResultType.FoundNoPlayer)
                return;

            var player = playerSearchResultData.Player;

            var diff = DateTime.Now - player.LastSeen;
            var diffReadable = $"Days: {diff.Days}, Hours: {diff.Hours}, Minutes: {diff.Minutes}, Seconds: {diff.Seconds}";

            player.AddResourcesGeneratedFromStructures();

            Logger.LogRaw(
                $"\nDATABASE" +
                $"\nUsername: {player.Username} " +
                $"\nGold: {player.ResourceCounts[Common.Game.ResourceType.Gold]}" +
                $"\nLumber Yards: {player.StructureCounts[Common.Game.StructureType.LumberYard]}" +
                $"\nLast Seen: {diffReadable}"
            );
        }
    }
}
