namespace GameServer.Logging.Commands
{
    public interface ICommand
    {
        string Description { get; set; }
        string Usage { get; set; }
        string[] Aliases { get; set; }

        void Run(string[] args);
    }
}
