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
        public static MongoClient Client { get; set; }
        public static IMongoDatabase Db { get; set; }

        public static void Connect() 
        {
            var authFileName = "auth.txt";
            var authFullPath = $"{Directory.GetCurrentDirectory()}\\{authFileName}";

            if (!File.Exists(authFileName))
            {
                Logger.LogError($"The following file does not exist and needs to be created: {authFullPath}");
                ExitApp();
                return;
            }

            var auth = File.ReadAllLines(authFileName);
            var username = auth[0];
            var password = auth[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Logger.LogError($"MongoDb username or password not defined in auth.txt in {authFullPath}");
                ExitApp();
                return;
            }

            var settings = MongoClientSettings.FromConnectionString($"mongodb://{username.Trim()}:{password.Trim()}@localhost:27017/admin?authSource=admin");
            Client = new MongoClient(settings);

            try
            {
                var database = Client.GetDatabase("admin");

                var connected = database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}").Wait(1000);

                if (!connected) 
                {
                    ExitApp();
                    return;
                }
            }
            catch (Exception e) 
            {
                Logger.LogError($"{e.Message}\n\nPerhaps the wrong username and password were used in {authFullPath}");
                ExitApp();
                return;
            }

            Db = Client.GetDatabase("database");
        }

        private static async void ExitApp() 
        {
            Logger.Log("Failed to connect to database");
            Logger.LogRaw("\nExiting application in 3 seconds...");
            await Task.Delay(3000);
            Environment.Exit(0);
        }
    }
}
