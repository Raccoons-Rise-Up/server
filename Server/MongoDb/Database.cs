using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Console;
using MongoDB.Driver;

namespace GameServer.Server.MongoDb
{
    public class Database
    {
        public static MongoClient DbClient;

        public static bool Connect() 
        {
            var authFileName = "auth.txt";
            var authFullPath = $"{Directory.GetCurrentDirectory()}\\{authFileName}";

            if (!File.Exists(authFileName))
            {
                Logger.LogError($"The following file does not exist and needs to be created: {authFullPath}");
                return false;
            }

            var auth = File.ReadAllLines(authFileName);
            var username = auth[0];
            var password = auth[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Logger.LogError($"MongoDb username or password not defined in auth.txt in {authFullPath}");
                return false;
            }

            var settings = MongoClientSettings.FromConnectionString($"mongodb://{username.Trim()}:{password.Trim()}@localhost:27017/admin?authSource=admin");
            DbClient = new MongoClient(settings);

            try
            {
                var database = DbClient.GetDatabase("admin");
                return database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}").Wait(1000);
            }
            catch (Exception e) 
            {
                Logger.LogError($"{e.Message}\n\nPerhaps the wrong username and password were used in {authFullPath}");
                return false;
            }
        }
    }
}
