using System;

namespace GameServer.Console.Commands
{
    public class CommandClear : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandClear()
        {
            Description = "Clear the console";
            Aliases = new string[] { "cls" };
        }

        public override void Run(string[] args)
        {
            Logger.TextField.row = 1;
            Logger.TextField.column = 0;
            Logger.LoggerMessageRow = 0;
            System.Console.CursorLeft = 0;
            System.Console.CursorTop = 0;
            System.Console.Clear();
        }
    }
}
