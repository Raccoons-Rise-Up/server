namespace GameServer.Logging.Commands
{
    public abstract class Command
    {
        public abstract string Description { get; set; }
        public abstract string Usage { get; set; }
        public abstract string[] Aliases { get; set; }

        public abstract void Run(string[] args);

        public override string ToString()
        {
            var cmdName = GetType().Name.ToLower().Replace("command", "");

            if (Description == null)
                Description = "No description defined";

            if (Usage == null)
                Usage = "No usage defined";

            return $"\n{cmdName.ToUpper()}" +
                $"\nDesc: {Description}" +
                $"\nUsage: {Usage}";
        }
    }
}
