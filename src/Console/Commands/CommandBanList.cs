using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameServer.Database;
using GameServer.Server;
using GameServer.Utilities;

namespace GameServer.Logging.Commands
{
    public class CommandBanList : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandBanList()
        {
            Description = "Show the list of banned players";
        }

        public override void Run(string[] args)
        {
            var bannedPlayers = Utils.ReadJSONFile<Dictionary<string, BannedPlayer>>("banned_players");
            var bannedPlayerNames = new List<string>();
            foreach (var player in bannedPlayers)
                bannedPlayerNames.Add(player.Value.Name);

            Logger.Log($"Banned Players: {string.Join(' ', bannedPlayerNames)}");
        }
    }
}
