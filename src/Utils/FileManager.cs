using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace GameServer.Utilities
{
    public static class FileManager
    {
        private static readonly string ResPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "res");

        public static void SetupDirectories() 
        {
            Directory.CreateDirectory(GetPath(ResPath, "Config"));
            Directory.CreateDirectory(GetPath(ResPath, "Data/ResourceIcons"));
        }

        public static byte[] ReadImageIcon(string filename) 
        {
            var pathToFile = GetPath(ResPath, $"Data/ResourceIcons/{filename}.png");
            var image = Image.FromFile(pathToFile);
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();
        }

        public static void CreateImageIcon(byte[] bytes, string path) 
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(bytes))
            {
                bmp = new Bitmap(ms);
            }

            bmp.Save($".\\{path}.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public static async void CreateConfig(string path, ConfigType configType = ConfigType.Object) 
        {
            var pathToFile = GetPath(ResPath, $"Config/{path}.json");

            TryCreateDirectoriesToFile(pathToFile);

            if (configType == ConfigType.Array)
                await File.WriteAllTextAsync(pathToFile, "[]");
            else
                await File.WriteAllTextAsync(pathToFile, "{}");
        }

        public static async void WriteConfig<T>(string path, T data)
        {
            var pathToFile = GetPath(ResPath, $"Config/{path}.json");

            TryCreateDirectoriesToFile(pathToFile);

            var options = new JsonSerializerOptions { WriteIndented = true };

            await File.WriteAllTextAsync(pathToFile, JsonSerializer.Serialize(data, options));
        }

        public static T ReadConfig<T>(string path) 
        {
            var pathToFile = GetPath(ResPath, $"Config/{path}.json");

            TryCreateDirectoriesToFile(pathToFile);

            var text = File.ReadAllText(pathToFile);
            return JsonSerializer.Deserialize<T>(text);
        }

        public static List<string> GetAllConfigNamesInFolder(string path = "")
        {
            var pathToFolder = GetPath(ResPath, $"Config/{path}");

            Directory.CreateDirectory(pathToFolder);

            return Directory.GetFiles(pathToFolder, "*.json").Select(Path.GetFileName).Select(x => x.Replace(".json", "")).ToList();
        }

        private static string GetPath(string dir, string path) => Path.Combine(dir, EnsureValidPath(path));

        private static string EnsureValidPath(string path) => path.Replace('/', Path.DirectorySeparatorChar);

        private static void TryCreateDirectoriesToFile(string pathToFile) 
        {
            Directory.CreateDirectory(Directory.GetParent(pathToFile).FullName);
        }

        public enum ConfigType 
        {
            Object,
            Array
        }
    }
}
