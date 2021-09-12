﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server;
using GameServer.Logging;

namespace GameServer.Utilities
{
    public static class PlayerManager
    {
        public static void UpdatePlayerConfig(Player player)
        {
            // Check if player exists in configs
            var dbPlayers = FileManager.GetAllConfigNamesInFolder("Players");
            var dbPlayer = dbPlayers.Find(str => str == player.Username);

            if (dbPlayer == null)
            {
                // Player does not exist in database, lets add them to the database
                //ConfigManager.CreateConfig($"Players/{player.Username}");
                FileManager.WriteConfig($"Players/{player.Username}", player);
                Logger.Log($"Player '{player.Username}' config created");
                return;
            }

            // Player exists in the database, lets update the config
            FileManager.WriteConfig($"Players/{player.Username}", player);
            Logger.Log($"Player '{player.Username}' config updated");
        }

        public static Player GetPlayerConfig(string username) 
        {
            var playerNames = FileManager.GetAllConfigNamesInFolder("Players");
            var playerName = playerNames.Find(str => str == username);

            if (playerName == null) 
                return null;

            return FileManager.ReadConfig<Player>($"Players/{playerName}");
        }
    }
}