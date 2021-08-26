using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameServer.Utilities
{
    public static class Utils
    {
        private static readonly string pathToRes = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, $"res");

        public static List<T> ReadJSONFile<T>(string filename)
        {
            CreateJSONFile(filename);

            var pathToFile = Path.Combine(pathToRes, filename + ".json");

            var text = File.ReadAllText(pathToFile);
            return JsonSerializer.Deserialize<List<T>>(text);
        }

        public static void WriteToJSONFile<T>(string filename, List<T> list)
        {
            CreateJSONFile(filename);

            var pathToFile = Path.Combine(pathToRes, filename + ".json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            File.WriteAllText(pathToFile, JsonSerializer.Serialize(list, options));
        }

        public static void CreateJSONFile(string filename)
        {
            var pathToFile = Path.Combine(pathToRes, filename + ".json");

            if (!File.Exists(pathToFile))
            {
                var fs = File.Create(pathToFile);
                fs.Close();
            }

            // Make sure json file has valid json tokens
            if (File.ReadAllText(pathToFile) == "") // Only write if nothing in file (note that "" is returned if nothing is in the file)
            {
                string json = JsonSerializer.Serialize(new List<string>());
                File.WriteAllText(pathToFile, json);
            }
        }
    }
}
