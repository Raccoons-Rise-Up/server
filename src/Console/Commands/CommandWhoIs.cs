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
                $"\nUsername: {player.Username} " +
                $"\nLast Seen: {diffReadable}" +
                GetPlayerResourcesAsString(player) +
                GetPlayerStructuresAsString(player)
            );
        }

        private static string GetPlayerStructuresAsString(Player player) 
        {
            var structuresReadable = "\n\n--- Structures ---";

            foreach (var structure in player.StructureCounts) 
            {
                var structureName = Enum.GetName(typeof(Common.Game.StructureType), structure.Key);
                var structureValue = structure.Value;

                structuresReadable += $"\n{structureName}: {structureValue}";
            }

            return structuresReadable;
        }

        private static string GetPlayerResourcesAsString(Player player) 
        {
            var resourcesReadable = "\n\n--- Resources ---";

            foreach (var resource in player.ResourceCounts)
            {
                var resourceName = Enum.GetName(typeof(Common.Game.ResourceType), resource.Key);
                var resourceValue = resource.Value;

                resourcesReadable += $"\n{resourceName}: {resourceValue}";
            }

            return resourcesReadable;
        }
    }
}
