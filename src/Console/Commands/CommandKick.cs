using GameServer.Server;

namespace GameServer.Logging.Commands
{
    public class CommandKick : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandKick()
        {
            Description = "Kick a player";
            Usage = "<player>";
        }

        public override void Run(string[] args) 
        {
            if (args.Length == 0) 
            {
                Logger.Log("Please provide a player name to kick");
                return;
            }

            var cmd = new ENetCmds();
            cmd.Set(ServerOpcode.KickPlayer, args[0]);

            ENetServer.ENetCmds.Enqueue(cmd);
        }
    }
}
