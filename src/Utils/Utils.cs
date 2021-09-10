using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameServer.Database;
using GameServer.Server;
using GameServer.Logging;
using GameServer.Server.Packets;

namespace GameServer.Utilities
{
    public static class Utils
    {
        public static string AddSpaceBeforeEachCapital(string str) => string.Concat(str.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        public static T ReadJSONString<T>(string str) => JsonSerializer.Deserialize<T>(str);

        public static T ReadJSONFile<T>(string filename)
        {
            CreateJSONDictionaryFile(filename);

            var pathToFile = Path.Combine(ENetServer.AppDataPath, filename + ".json");

            var text = File.ReadAllText(pathToFile);
            return JsonSerializer.Deserialize<T>(text);
        }

        public static void WriteToJSONFile<T>(string filename, T data)
        {
            CreateJSONDictionaryFile(filename);

            var pathToFile = Path.Combine(ENetServer.AppDataPath, filename + ".json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            File.WriteAllText(pathToFile, JsonSerializer.Serialize(data, options));
        }

        public static void CreateJSONDictionaryFile(string filename)
        {
            var pathToFile = Path.Combine(ENetServer.AppDataPath, filename + ".json");

            if (!File.Exists(pathToFile))
            {
                var fs = File.Create(pathToFile);
                fs.Close();
            }

            // Make sure json file has valid json tokens
            if (File.ReadAllText(pathToFile) == "") // Only write if nothing in file (note that "" is returned if nothing is in the file)
            {
                string json = JsonSerializer.Serialize(new Dictionary<int, int>());
                File.WriteAllText(pathToFile, json);
            }
        }

        public static void AddPlayerToBanList(Player player)
        {
            var bannedPlayers = Utils.ReadJSONFile<Dictionary<string, BannedPlayer>>("banned_players");

            if (bannedPlayers.ContainsKey(player.Ip))
            {
                Logger.Log($"Player '{player.Username}' was banned already");
                return;
            }

            bannedPlayers[player.Ip] = new BannedPlayer()
            {
                Name = player.Username
            };

            Utils.WriteToJSONFile("banned_players", bannedPlayers);

            Logger.Log($"Player '{player.Username}' was banned");
        }

        public static void RemovePlayerFromBanList(ModelPlayer player)
        {
            var bannedPlayers = Utils.ReadJSONFile<Dictionary<string, BannedPlayer>>("banned_players");

            if (!bannedPlayers.ContainsKey(player.Ip))
            {
                Logger.Log($"Player '{player.Username}' was pardoned already");
                return;
            }

            bannedPlayers.Remove(player.Ip);

            Utils.WriteToJSONFile("banned_players", bannedPlayers);

            Logger.Log($"Player '{player.Username}' was pardoned");
        }

        public static void PardonOfflinePlayer(string username)
        {
            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();
            var dbPlayer = dbPlayers.Find(x => x.Username == username);

            if (dbPlayer == null)
            {
                Logger.Log($"No player with the username '{username}' could be found to be pardoned");
                return;
            }

            RemovePlayerFromBanList(dbPlayer);
        }

        public static bool BanOfflinePlayer(string username)
        {
            using var db = new DatabaseContext();

            var dbPlayers = db.Players.ToList();
            var dbPlayer = dbPlayers.Find(x => x.Username == username);

            if (dbPlayer == null)
                return false;

            var player = new Player
            {
                Username = dbPlayer.Username,
                Ip = dbPlayer.Ip
            };

            AddPlayerToBanList(player);

            return true;
        }

        public static bool BanOnlinePlayer(string username)
        {
            var player = ENetServer.Players.Find(x => x.Username == username);
            if (player == null)
                return false;

            player.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);

            AddPlayerToBanList(player);

            ENetServer.Players.Remove(player);

            return true;
        }
    }

    public struct BannedPlayer
    {
        public string Name { get; set; }
    }
}
