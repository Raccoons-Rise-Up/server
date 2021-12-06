using GameServer.Server;
using GameServer.Utilities;

namespace GameServer.Logging.Commands
{
    public class CommandBan : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandBan() 
        {
            Description = "Ban a specified player";
            Usage = "<player>";
        }

        public override void Run(string[] args) 
        {
            if (args.Length == 0)
            {
                Logger.Log("Please provide a player name to ban");
                return;
            }

            Player.BanPlayer(args[0]);
        }
    }
}
