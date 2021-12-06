using GameServer.Server;
using GameServer.Utilities;

namespace GameServer.Logging.Commands
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

            BanManager.PardonOfflinePlayer(args[0]);
        }
    }
}
