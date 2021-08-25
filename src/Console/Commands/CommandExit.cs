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
        public override async void Run(string[] args) 
        {
            ENetServer.SaveAllPlayersToDatabase();

            Logger.Log("Exiting application in 5 seconds...");
            await Task.Delay(5000);
            Environment.Exit(0);
        }
    }
}
