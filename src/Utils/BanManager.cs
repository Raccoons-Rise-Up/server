using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Logging;
using GameServer.Server;
using GameServer.Server.Packets;

namespace GameServer.Utilities
{
    public static class BanManager
    {
        public static void AddPlayerToBanList(Player player)
        {
            var bannedPlayers = FileManager.ReadConfig<Dictionary<string, BannedPlayer>>("banned_players");

            if (bannedPlayers.ContainsKey(player.Peer.IP))
            {
                Logger.Log($"Player '{player.Username}' was banned already");
                return;
            }

            bannedPlayers[player.Peer.IP] = new BannedPlayer()
            {
                Name = player.Username
            };

            FileManager.WriteConfig("banned_players", bannedPlayers);

            Logger.Log($"Player '{player.Username}' was banned");
        }

        public static void RemovePlayerFromBanList(Player player) // TODO
        {
            var bannedPlayers = FileManager.ReadConfig<Dictionary<string, BannedPlayer>>("banned_players");

            if (!bannedPlayers.ContainsKey(player.Peer.IP))
            {
                Logger.Log($"Player '{player.Username}' was pardoned already");
                return;
            }

            bannedPlayers.Remove(player.Peer.IP);

            FileManager.WriteConfig("banned_players", bannedPlayers);

            Logger.Log($"Player '{player.Username}' was pardoned");
        }

        public static void PardonOfflinePlayer(string username)
        {
            var bannedPlayers = FileManager.ReadConfig<List<BannedPlayer>>("banned_players");
            var bannedPlayer = bannedPlayers.Find(x => x.Name == username);

            if (bannedPlayer == null)
            {
                Logger.Log($"No player with the username '{username}' could be found to be pardoned");
                return;
            }

            bannedPlayers.Remove(bannedPlayer);

            FileManager.WriteConfig("banned_players", bannedPlayers);
        }

        public static bool BanOfflinePlayer(string username)
        {
            /*var bannedPlayers = ConfigManager.ReadConfig<List<BannedPlayer>>("banned_players");
            var bannedPlayer = bannedPlayers.Find(x => x.Name == username);

            if (bannedPlayer == null) 
            {
                bannedPlayers.Add(new BannedPlayer {
                    Name
                });
            }*/

            return true;
        }

        public static bool BanOnlinePlayer(string username)
        {
            Player player = null;
            foreach (var p in ENetServer.Players.Values) 
            {
                if (p.Username == username)
                    player = p;
            }

            if (player == null)
                return false;

            player.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);

            AddPlayerToBanList(player);

            ENetServer.Players.Remove(player.Peer.ID);

            return true;
        }
    }

    public class BannedPlayer
    {
        public string Ip { get; set; }
        public string Name { get; set; }
    }
}
