using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GameServer.Server;
using GameServer.Utilities;

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
            if (args.Length == 0)
            {
                foreach (var player in Player.GetAllPlayerConfigs())
                    player.ResetValues();

                Logger.Log("Reset values for all players");
                return;
            }

            //var dbPlayer = db.Players.ToList().Find(x => x.Username == args[0]);

            /*if (dbPlayer == null)
            {
                Logger.Log($"The player with the username '{args[0]}' could not be found in the database ({stopwatch.ElapsedMilliseconds} ms)");
                return;
            }*/

            //ResetPlayer(db, dbPlayer);
        }

        /*private static void ResetPlayer(DatabaseContext db, ModelPlayer dbPlayer) 
        {
            // Remove the player from the database
            db.Remove(dbPlayer);
            db.SaveChanges();

            // Clear the players variables from players list
            var cmd = new ENetCmds();
            cmd.Set(ServerOpcode.ClearPlayerStats, dbPlayer.Username);

            ENetServer.ENetCmds.Enqueue(cmd);

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
        }*/
    }
}
