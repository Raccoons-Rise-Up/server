using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GameServer.Server;
using GameServer.Utils;
using GameServer.Server.Game;

namespace GameServer.Console.Commands
{
    public class CommandReset : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandReset() 
        {
            Description = "Clear a specific players stats or the entire database";
            Usage = "[player]";
        }

        public override void Run(string[] args) 
        {
            /*if (args.Length == 0)
            {
                foreach (var p in ServerPlayer.GetAllConfigs())
                    p.ResetValues();

                Logger.Log("Reset values for all player configs");
                return;
            }

            var playerSearchResultData = ServerUtils.FindPlayer(args[0]);

            if (playerSearchResultData.SearchResult == ServerUtils.PlayerSearchResultType.FoundNoPlayer)
                return;

            var player = playerSearchResultData.Player;
            player.ResetValues();*/
        }
    }
}
