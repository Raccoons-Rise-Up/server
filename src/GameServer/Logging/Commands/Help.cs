using System;
using System.Linq;

namespace GameServer.Logging.Commands
{
    public class Help : Command
    {
        // Loads available commands into array at runtime
        protected static string[] commands = typeof(Command).Assembly.GetTypes()
                        .Where(x => typeof(Command).IsAssignableFrom(x) && !x.IsAbstract)
                        .Select(Activator.CreateInstance).Cast<Command>()
                        .Aggregate("", (str, x) => str + x.GetType().Name.ToLower() + " ")
                        .Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();

        public override void Run(string[] args)
        {
            Logger.Log($"Commands:\n - {String.Join("\n - ", commands)}");
        }
    }
}