using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameServer.Database;
using GameServer.Server;

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

            FindPlayer(args);
        }

        private static void FindPlayer(string[] args) 
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();
            var dbPlayer = dbPlayers.Find(x => x.Username == args[0]);

            stopwatch.Stop();

            if (dbPlayer == null)
            {
                Logger.Log($"Could not find any player with the username '{args[0]}' ({stopwatch.ElapsedMilliseconds} ms)");
                return;
            }

            Logger.LogRaw($"\nFound player with username '{args[0]}' ({stopwatch.ElapsedMilliseconds} ms)");

            PlayerFromDatabase(dbPlayer);
            PlayerFromCache(dbPlayer.Username);
        }

        private static void PlayerFromCache(string username) 
        {
            var cmd = new ENetCmds();
            cmd.Set(ServerOpcode.GetPlayerStats, username);

            ENetServer.ENetCmds.Enqueue(cmd);
        }

        private static void PlayerFromDatabase(ModelPlayer dbPlayer) 
        {
            var diff = DateTime.Now - dbPlayer.LastSeen;
            var diffReadable = $"Days: {diff.Days}, Hours: {diff.Hours}, Minutes: {diff.Minutes}, Seconds: {diff.Seconds}";

            Logger.LogRaw(
                $"\nDATABASE" +
                $"\nUsername: {dbPlayer.Username} " +
                $"\nGold: {dbPlayer.ResourceGold} (+{CalculateGoldGeneratedFromStructures(dbPlayer)})" +
                $"\nStructure Huts: {dbPlayer.StructureHut}" +
                $"\nLast Seen: {diffReadable}"
            );
        }

        private static uint CalculateGoldGeneratedFromStructures(ModelPlayer player)
        {
            // Calculate players new gold value based on how many structures they own
            var diff = DateTime.Now - player.LastCheckStructureHut;
            uint goldGenerated = player.StructureHut * (uint)diff.TotalSeconds;

            player.LastCheckStructureHut = DateTime.Now;

            return goldGenerated;
        }
    }
}
