using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Server;
using GameServer.Logging;

namespace GameServer.Utilities
{
    public static class ServerUtils
    {
        /*
         * Checks to see if a player with the given username is online and returns that
         * player, if not checks to see if a player with the given username is offline
         * and returns that player config
         */
        public static PlayerSearchResult FindPlayer(string username) 
        {
            // Check to see if the player is online
            Player player = null;
            var isOffline = false;
            foreach (var p in ENetServer.Players.Values) 
            {
                if (p.Username.Equals(username))
                    player = p;
            }

            if (player == null) 
            {
                // Could not find a player that was online
                // Lets check to see if the player is in the offline configs
                var offlinePlayerConfigs = Player.GetAllPlayerConfigs();
                foreach (var p in offlinePlayerConfigs) 
                {
                    if (p.Username.Equals(username))
                        player = p;
                }

                if (player == null) 
                {
                    // Could not find a player that was offline
                    Logger.Log($"Player by the username '{username}' was not found");
                    return new PlayerSearchResult 
                    {
                        SearchResult = PlayerSearchResultType.FoundNoPlayer,
                        Player = null
                    };
                }

                // Found a player that was offline with this username
                isOffline = true;
            }

            // If the code has reached this point we have either found a online or offline player
            return new PlayerSearchResult
            {
                SearchResult = isOffline ? PlayerSearchResultType.FoundOfflinePlayer : PlayerSearchResultType.FoundOnlinePlayer,
                Player = player
            };
        }

        public struct PlayerSearchResult 
        {
            public PlayerSearchResultType SearchResult { get; set; }
            public Player Player { get; set; }
        }

        public enum PlayerSearchResultType
        {
            FoundOnlinePlayer,
            FoundOfflinePlayer,
            FoundNoPlayer
        }
    }
}
