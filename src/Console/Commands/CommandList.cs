using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GameServer.Server;
using GameServer.Database;

namespace GameServer.Logging.Commands
{
    public class CommandList : ICommand
    {
        public string Description { get; set; }
        public string Usage { get; set; }
        public string[] Aliases { get; set; }

        public CommandList() 
        {
            Description = "Get a list of all offline or online players";
            Usage = "[offline]";
        }

        public void Run(string[] args)
        {
            if (args.Length == 0) 
            {
                GetOnlinePlayers();
                return;
            }

            GetOfflinePlayers();
        }

        private static void GetOnlinePlayers() 
        {
            var cmd = new ServerInstructions(ServerInstructionOpcode.GetOnlinePlayers);
            ENetServer.ServerInstructions.Enqueue(cmd);
        }

        private static void GetOfflinePlayers() 
        {
            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();

            if (dbPlayers.Count == 0)
            {
                Logger.Log("There are 0 players in the database");
                return;
            }

            Logger.LogRaw($"\nOffline Players: {string.Join(' ', dbPlayers)}");
        }
    }
}
