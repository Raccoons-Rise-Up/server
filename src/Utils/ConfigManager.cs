using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace GameServer.Utilities
{
    public static class ConfigManager
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ENet Server");

        public static void CreateAppDataFolder() 
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);
        }

        public static void CreateConfig(string path, ConfigType configType = ConfigType.Object)
        {
            var pathToFile = GetPath(path) + ".json";

            // Create the file
            if (!File.Exists(pathToFile))
                File.Create(pathToFile).Close();

            // Make sure the file is valid json file
            if (configType == ConfigType.Object)
                File.WriteAllText(pathToFile, "{}");

            if (configType == ConfigType.Array)
                File.WriteAllText(pathToFile, "[]");
        }

        public static void WriteConfig<T>(string path, T data)
        {
            var pathToFile = GetPath(path) + ".json";
            var options = new JsonSerializerOptions { WriteIndented = true };

            File.WriteAllText(pathToFile, JsonSerializer.Serialize(data, options));
        }

        public static T ReadConfig<T>(string path) 
        {
            var pathToFile = GetPath(path) + ".json";

            var text = File.ReadAllText(pathToFile);
            return JsonSerializer.Deserialize<T>(text);
        }

        public static List<string> GetAllConfigNamesInFolder(string path = "")
        {
            var pathToFile = GetPath(path);

            // Ensure all directories are created
            Directory.CreateDirectory(pathToFile);

            return Directory.GetFiles(pathToFile, "*.json").Select(Path.GetFileName).Select(x => x.Replace(".json", "")).ToList();
        }

        private static string GetPath(string path) 
        {
            // Ensure the proper file seperator characters are used
            path = EnsureValidPath(path);

            // Get the path to the file
            return Path.Combine(AppDataPath, path);
        }

        private static string EnsureValidPath(string path) => path.Replace('/', Path.DirectorySeparatorChar);

        public enum ConfigType 
        {
            Object,
            Array
        }
    }
}
