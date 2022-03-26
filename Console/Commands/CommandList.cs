using System.Linq;
using GameServer.Server;

namespace GameServer.Console.Commands
{
    public class CommandList : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandList() 
        {
            Description = "Get a list of all offline or online players";
            Usage = "[offline]";
        }

        public override void Run(string[] args)
        {
            GetOnlinePlayers();
        }

        private static void GetOnlinePlayers() 
        {
            ENetServer.ENetCmds.Enqueue(new ENetCommand { Opcode = ENetOpcode.GetOnlinePlayers });
        }
    }
}
