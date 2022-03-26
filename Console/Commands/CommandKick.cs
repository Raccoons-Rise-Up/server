using System.Collections.Generic;
using GameServer.Server;

namespace GameServer.Console.Commands
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

            ENetServer.ENetCmds.Enqueue(new ENetCommand { Opcode = ENetOpcode.KickPlayer, Data = new List<object> { args[0] } });
        }
    }
}
