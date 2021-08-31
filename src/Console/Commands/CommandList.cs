using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GameServer.Server;
using GameServer.Database;

namespace GameServer.Logging.Commands
{
    public class CommandList : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandList() 
        {
            Description = "Get a list of all offline or online players";
            Usage = "[offline]";
        }

        public override void Run(string[] args)
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
            var cmd = new ENetCmds(ServerOpcode.GetOnlinePlayers);
            ENetServer.ENetCmds.Enqueue(cmd);
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
