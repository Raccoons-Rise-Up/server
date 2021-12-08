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
            Player player = FindOnlinePlayer(username);

            if (player != null) // Found a online player
            {
                return new PlayerSearchResult
                {
                    SearchResult = PlayerSearchResultType.FoundOnlinePlayer,
                    Player = player
                };
            }

            // Could not find a online player
            // Lets check to see if the player exists in the offline configs
            player = FindOfflinePlayer(username);

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
            return new PlayerSearchResult
            {
                SearchResult = PlayerSearchResultType.FoundOfflinePlayer,
                Player = player
            };
        }

        public static Player FindOnlinePlayer(string username) 
        {
            foreach (var p in ENetServer.Players.Values)
                if (p.Username.Equals(username))
                    return p;

            return null;
        }

        public static Player FindOfflinePlayer(string username) 
        {
            foreach (var p in Player.GetAllPlayerConfigs())
                if (p.Username.Equals(username))
                    return p;

            return null;
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
