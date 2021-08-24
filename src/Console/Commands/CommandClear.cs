using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Database;
using GameServer.Server;

namespace GameServer.Logging.Commands
{
    public class CommandClear : Command
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

            if (dbPlayer == null)
            {
                Logger.Log($"The player with the username '{args[0]}' can not be found in the database");
                return;
            }

            // Remove the player from the database
            db.Remove(dbPlayer);
            db.SaveChanges();

            // Clear the players variables from players list
            var cmd = new ServerInstructions();
            cmd.Set(ServerInstructionOpcode.ClearPlayerStats, dbPlayer.Username);

            ENetServer.serverInstructions.Enqueue(cmd);

            Logger.Log($"Cleared {dbPlayer.Username} from database");
        }
    }
}
