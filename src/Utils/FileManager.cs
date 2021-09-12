using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;

namespace GameServer.Utilities
{
    public static class FileManager
    {
        private static readonly string ResPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "res");

        public static void CreateConfig(string path, ConfigType configType = ConfigType.Object)
        {
            WriteConfig(path, configType == ConfigType.Array ? "[]" : "{}");
        }

        public static async void WriteConfig<T>(string path, T data)
        {
            var pathToFile = GetPath(ResPath, $"Config/{path}.json");

            TryCreateDirectories(pathToFile);

            var options = new JsonSerializerOptions { WriteIndented = true };

            await File.WriteAllTextAsync(pathToFile, JsonSerializer.Serialize(data, options));
        }

        public static T ReadConfig<T>(string path) 
        {
            var pathToFile = GetPath(ResPath, $"Config/{path}");

            TryCreateDirectories(pathToFile);

            var text = File.ReadAllText(pathToFile);
            return JsonSerializer.Deserialize<T>(text);
        }

        public static List<string> GetAllConfigNamesInFolder(string path = "")
        {
            var pathToFile = GetPath(ResPath, $"Config/{path}");

            TryCreateDirectories(pathToFile);

            return Directory.GetFiles(pathToFile, "*.json").Select(Path.GetFileName).Select(x => x.Replace(".json", "")).ToList();
        }

        private static string GetPath(string dir, string path) => Path.Combine(dir, EnsureValidPath(path));

        private static string EnsureValidPath(string path) => path.Replace('/', Path.DirectorySeparatorChar);

        private static void TryCreateDirectories(string pathToFile) 
        {
            var parent = Directory.GetParent(pathToFile);
            if (!parent.Exists)
                Directory.CreateDirectory(parent.FullName);
        }

        public enum ConfigType 
        {
            Object,
            Array
        }
    }
}
