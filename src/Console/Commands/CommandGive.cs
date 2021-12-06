using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server;
using Common.Game;
using Common.Utils;

namespace GameServer.Logging.Commands
{
    public class CommandGive : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandGive() 
        {
            Description = "Give resources to a player";
            Usage = "<player> <resource> <amount>";
        }

        public override void Run(string[] args) 
        {
            if (args.Length == 0) 
            {
                Logger.Log($"Usage: give {Usage}");
                return;
            }

            Player player = null;
            foreach (var p in ENetServer.Players.Values)
            {
                if (p.Username == args[0])
                    player = p;
            }

            if (player == null) 
            {
                Logger.Log($"Player by the username '{args[0]}' was not found");
                return;
            }
            
            if (args.Length < 2) 
            {
                Logger.Log("Please specify a resource type");
                return;
            }

            if (!Enum.TryParse(args[1].ToTitleCase(), out ResourceType resourceType))
            {
                Logger.Log($"'{args[1].ToTitleCase()}' is not a valid resource type");
                return;
            }

            if (args.Length < 3) 
            {
                Logger.Log("Please specify an amount");
                return;
            }

            if (!double.TryParse(args[2], out double amount))
            {
                Logger.Log($"'{args[2]}' is not a valid number");
                return;
            }

            player.ResourceCounts[resourceType] = player.ResourceCounts[resourceType] + amount;

            Logger.Log($"Player '{player.Username}' now has {player.ResourceCounts[resourceType] + amount} {resourceType}");
        }
    }
}
