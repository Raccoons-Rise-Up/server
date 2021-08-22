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

            var diff = DateTime.Now - player.LastSeen;
            var diffReadable = $"Days: {(int)diff.Days}, Hours: {(int)diff.Hours}, Minutes: {(int)diff.Minutes}, Seconds: {(int)diff.Seconds}";

            Logger.Log(
                $"\nUsername: {player.Username} " +
                $"\nGold: {player.Gold}" +
                $"\nStructure Huts: {player.StructureHut}" +
                $"\nLast Seen: {diffReadable}"
            );
        }
    }
}
