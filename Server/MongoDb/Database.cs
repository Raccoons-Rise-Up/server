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
        public Database() 
        {

        }

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

            var settings = MongoClientSettings.FromConnectionString($"mongodb://{username.Trim()}:{password.Trim()}@localhost:27017");
            var client = new MongoClient(settings);

            try
            {
                var dbList = client.ListDatabases().ToList();

                Logger.Log("The list of databases on this server is: ");
                foreach (var db in dbList)
                {
                    Logger.Log(db);
                }

                return true;
            }
            catch (Exception e) 
            {
                Logger.LogError($"{e.Message}\n\nPerhaps the wrong username and password were used in {authFullPath}");
                return false;
            }
        }
    }
}
