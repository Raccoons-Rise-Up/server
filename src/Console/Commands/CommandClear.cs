using System;
using System.Collections.Generic;
using GameServer.Logging;

namespace GameServer.Logging.Commands
{
    public class CommandClear : ICommand
    {
        public string Description { get; set; }
        public string Usage { get; set; }
        public string[] Aliases { get; set; }

        public CommandClear()
        {
            Description = "Clear the console";
            Aliases = new string[] { "cls" };
        }

        public void Run(string[] args)
        {
            Logger.TextField.row = 1;
            Logger.TextField.column = 0;
            Logger.LoggerMessageRow = 0;
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            Console.Clear();
        }
    }
}
