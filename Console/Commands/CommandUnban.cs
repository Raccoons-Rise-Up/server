using GameServer.Server;
using GameServer.Server.Game;
using GameServer.Utils;

namespace GameServer.Console.Commands
{
    public class CommandUnban : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandUnban()
        {
            Description = "Pardon a specified player";
            Usage = "<player>";
            Aliases = new string[] { "pardon" };
        }

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                Logger.Log("Please provide a player name to pardon");
                return;
            }

            PlayerUtils.PardonOffline(args[0]);
        }
    }
}
