using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Database;
using GameServer.Server;

namespace GameServer.Logging.Commands
{
    public class CommandWhoIs : Command
    {
        public override void Run(string[] args) 
        {
            if (args.Length == 0) 
            {
                Logger.Log("Please provide a username to search for");
                return;
            }

            using var db = new DatabaseContext();

            var dbPlayer = db.Players.ToList().Find(x => x.Username == args[0]);

            if (dbPlayer != null)
            {
                var diff = DateTime.Now - dbPlayer.LastSeen;
                var diffReadable = $"Days: {diff.Days}, Hours: {diff.Hours}, Minutes: {diff.Minutes}, Seconds: {diff.Seconds}";

                Logger.Log(
                    $"\nDATABASE" +
                    $"\nUsername: {dbPlayer.Username} " +
                    $"\nGold: {dbPlayer.Gold}" +
                    $"\nStructure Huts: {dbPlayer.StructureHut}" +
                    $"\nLast Seen: {diffReadable}"
                );
            }

            var player = ENetServer.players.Find(x => x.Username == args[0]);
            if (player == null)
                return;

            var diffX = DateTime.Now - player.LastSeen;
            var diffReadableX = $"Days: {diffX.Days}, Hours: {diffX.Hours}, Minutes: {diffX.Minutes}, Seconds: {diffX.Seconds}";

            Logger.Log(
                $"\n\nLIST" +
                $"\nUsername: {player.Username} " +
                $"\nGold: {player.Gold}" +
                $"\nStructure Huts: {player.StructureHut}" +
                $"\nLast Seen: {diffReadableX}"
            );
        }
    }
}
