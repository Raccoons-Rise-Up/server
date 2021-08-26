using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using GameServer.Database;
using GameServer.Server;

namespace GameServer.Logging.Commands
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
            using var db = new DatabaseContext();

            if (args.Length == 0)
            {
                ResetDatabase(db);
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var dbPlayer = db.Players.ToList().Find(x => x.Username == args[0]);

            stopwatch.Stop();

            if (dbPlayer == null)
            {
                Logger.Log($"The player with the username '{args[0]}' could not be found in the database ({stopwatch.ElapsedMilliseconds} ms)");
                return;
            }

            ResetPlayer(db, dbPlayer);
        }

        private static void ResetPlayer(DatabaseContext db, ModelPlayer dbPlayer) 
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Remove the player from the database
            db.Remove(dbPlayer);
            db.SaveChanges();

            // Clear the players variables from players list
            var cmd = new ServerInstructions();
            cmd.Set(ServerInstructionOpcode.ClearPlayerStats, dbPlayer.Username);

            ENetServer.ServerInstructions.Add(cmd);

            stopwatch.Stop();

            Logger.Log($"Cleared {dbPlayer.Username} from database ({stopwatch.ElapsedMilliseconds} ms)");
        }

        private static void ResetDatabase(DatabaseContext db) 
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            db.RemoveRange(db.Players);
            db.SaveChanges();

            stopwatch.Stop();
            Logger.Log($"Cleared database. There are {db.Players.Count()} players left in database ({stopwatch.ElapsedMilliseconds} ms)");
        }
    }
}
