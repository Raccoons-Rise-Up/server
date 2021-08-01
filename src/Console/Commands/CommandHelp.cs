using System;
using System.Linq;

namespace GameServer.Logging.Commands
{
    public class CommandHelp : Command
    {
        private static readonly string[] s_Commands = typeof(Command).Assembly.GetTypes()
            .Where(x => typeof(Command)
            .IsAssignableFrom(x) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<Command>()
            .Aggregate("", (str, x) => str + x.GetType().Name
            .ToLower() + " ")
            .Replace("command", "")
            .Split(' ')
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        public override void Run(string[] args)
        {
            Logger.Log($"Commands:\n - {string.Join("\n - ", s_Commands)}");
        }
    }
}
