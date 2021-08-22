using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Database;

namespace GameServer.Logging.Commands
{
    public class CommandWhoIs : Command
    {
        public override void Run(string[] args) 
        {
            using var db = new DatabaseContext();

            if (args.Length == 0) 
            {
                Logger.Log("Please provide a username to search for");
                return;
            }

            var player = db.Players.ToList().Find(x => x.Username == args[0]);

            if (player == null) 
            {
                Logger.Log($"The player with the username '{args[0]}' can not be found in the database");
                return;
            }

            Logger.Log($"Username: {player.Username} " +
                $"Last Seen: {player.LastSeen.Second} Seconds Ago");
        }
    }
}
