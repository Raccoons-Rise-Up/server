using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GameServer.Server;
using GameServer.Database;

namespace GameServer.Logging.Commands
{
    public class CommandExit : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandExit() 
        {
            Description = "Properly shutdown the server by saving everything";
            Aliases = new string[] { "stop", "quit" };
        }

        public async override void Run(string[] args)
        {
            ENetServer.SaveAllPlayersToDatabase();

            Logger.LogRaw("\nExiting application in 3 seconds...");
            await Task.Delay(3000);
            Environment.Exit(0);
        }
    }
}
