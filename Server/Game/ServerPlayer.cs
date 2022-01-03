using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Game;
using Common.Netcode;
using GameServer.Console;
using GameServer.Utils;
using ENet;

namespace GameServer.Server.Game
{
    public class ServerPlayer : Player
    {
        public ServerPlayer(Peer peer, string username) : base(peer, username) 
        {
        
        }

        public void SaveConfig() 
        {
            // Check if player exists in configs
            var dbPlayers = FileManager.GetAllConfigNamesInFolder("Players");
            var dbPlayer = dbPlayers.Find(str => str == Username);

            // Reminder: WriteConfig creates a config if no file exists
            FileManager.WriteConfig($"Players/{Username}", this);

            if (dbPlayer == null)
            {
                // Player does not exist in database, lets add them to the database
                Logger.Log($"Player '{Username}' config created");
                return;
            }

            // Player exists in the database, lets update the config
            Logger.Log($"Player '{Username}' config updated");
        }
    }
}
