using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server;

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
            if (args.Length < 3) 
            {
                Logger.Log(this);
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

            var resourceType = args[1];
            var amount = args[2];
        }
    }
}
