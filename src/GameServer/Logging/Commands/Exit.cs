namespace GameServer.Logging.Commands
{
    public class Exit : Command
    {
        public override void Run(string[] args)
        {
            Program.Server.Stop();
            System.Environment.Exit(0);
        }
    }
}