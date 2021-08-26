using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameServer.Database;
using GameServer.Server;
using GameServer.Utilities;

namespace GameServer.Logging.Commands
{
    public class CommandWhitelist : Command
    {
        public override string Description { get; set; }
        public override string Usage { get; set; }
        public override string[] Aliases { get; set; }

        public CommandWhitelist() 
        {
            Description = "Base command for editing the whitelist";
            Usage = "<add | remove> <player>";
        }

        public override void Run(string[] args) 
        {
            if (args.Length < 2) 
            {
                Logger.Log($"Usage: {Usage}");
                return;
            }

            // TODO: Whitelist
        }
    }
}
