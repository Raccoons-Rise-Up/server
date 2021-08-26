using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Logging;

namespace GameServer.Logging.Commands
{
    public class CommandHelp : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandHelp()
        {
            Description = "View a list of commands or get more information on a specific command";
            Usage = "[command]";
            Aliases = new string[] { "h" };
        }

        public override void Run(string[] args)
        {
            var cmds = Logger.Commands;

            if (args.Length == 0)
            {
                var stringArr = new string[cmds.Count];

                var i = 0;
                foreach (var cmd in cmds)
                {
                    if (cmd.Value.Aliases == null)
                        stringArr[i++] = $"{cmd.Key}";
                    else
                        stringArr[i++] = $"{cmd.Key} [{string.Join(' ', cmd.Value.Aliases)}]";
                }

                Logger.LogRaw(
                    $"\nCommands:" +
                    $"\n - {string.Join("\n - ", stringArr)}");

                return;
            }

            if (cmds.ContainsKey(args[0]))
                Logger.LogRaw(cmds[args[0]]);
            else
                Logger.Log($"Could not find command '{args[0]}'");
        }
    }
}
