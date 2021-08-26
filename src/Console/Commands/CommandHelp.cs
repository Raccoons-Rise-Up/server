using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Logging;

namespace GameServer.Logging.Commands
{
    public class CommandHelp : ICommand
    {
        public string Description { get; set; }
        public string Usage { get; set; }
        public string[] Aliases { get; set; }

        public CommandHelp()
        {
            Description = "View a list of commands or get more information on a specific command";
            Usage = "[command]";
            Aliases = new string[] { "h" };
        }

        public void Run(string[] args)
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

            GetInfoFromCommand(cmds, args[0]);
        }

        private static void GetInfoFromCommand(Dictionary<string, ICommand> cmds, string cmd)
        {
            if (cmds.ContainsKey(cmd))
                LogInfoFromCommand(cmds[cmd], cmd);
            else
            {
                var foundCmd = false;
                foreach (var command in cmds.Values)
                {
                    if (command.Aliases == null)
                        continue;

                    foreach (var alias in command.Aliases)
                    {
                        if (cmd.Equals(alias))
                        {
                            foundCmd = true;
                            LogInfoFromCommand(command, cmd);
                        }
                    }
                }

                if (!foundCmd)
                    Logger.Log($"Could not find a command with the name '{cmd}'");
            }
        }

        private static void LogInfoFromCommand(ICommand command, string cmd) 
        {
            var desc = command.Description;
            var usage = command.Usage;

            if (desc == null)
                desc = "No description defined";

            if (usage == null)
                usage = "No usage defined";

            Logger.LogRaw(
                $"\n{cmd.ToUpper()}" +
                $"\nDesc: {desc}" +
                $"\nUsage: {usage}");
        }
    }
}
