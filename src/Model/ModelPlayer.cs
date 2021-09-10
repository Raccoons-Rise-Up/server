using GameServer.Server;
using GameServer.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    public class ModelPlayer
    {
        private readonly Dictionary<string, PropertyInfo> propertyResources;
        private readonly Dictionary<string, StructurePropertyData> propertyStructures;

        public ModelPlayer() 
        {
            propertyResources = new();
            propertyStructures = new();

            var properties = typeof(ModelPlayer).GetProperties();

            foreach (var prop in properties.Where(x => x.Name.StartsWith("Resource")))
                propertyResources.Add(prop.Name.Replace("Resource", ""), prop);

            var propertiesStructure = new Dictionary<string, PropertyInfo>();
            var propertiesStructureLastCheck = new Dictionary<string, PropertyInfo>();

            foreach (var prop in properties.Where(x => x.Name.StartsWith("Structure")))
                propertiesStructure.Add(prop.Name.Replace("Structure", ""), prop);

            foreach (var prop in properties.Where(x => x.Name.StartsWith("LastCheckStructure")))
                propertiesStructureLastCheck.Add(prop.Name.Replace("LastCheckStructure", ""), prop);

            foreach (var prop in propertiesStructure)
            {
                propertyStructures.Add(Utils.AddSpaceBeforeEachCapital(prop.Key), new StructurePropertyData
                {
                    PropertyStructure = prop.Value,
                    PropertyStructureLastCheck = propertiesStructureLastCheck[prop.Key]
                });
            }
        }

        public Dictionary<string, PropertyInfo> PropertyResources => propertyResources;
        public Dictionary<string, StructurePropertyData> PropertyStructures => propertyStructures;

        public override string ToString() => Username;

        // DATABASE PROPERTIES
        // Generic
        public int ModelPlayerId { get; set; }
        public string Ip { get; set; }
        public string Username { get; set; }
        public DateTime LastSeen { get; set; }

        // Resources
        public uint ResourceWood { get; set; }
        public uint ResourceStone { get; set; }
        public uint ResourceWheat { get; set; }
        public uint ResourceGold { get; set; }

        // Structures
        public uint StructureHut { get; set; }
        public uint StructureWheatFarm { get; set; }

        // Structure Last Checks
        public DateTime LastCheckStructureHut { get; set; }
        public DateTime LastCheckStructureWheatFarm { get; set; }
    }

    public struct StructurePropertyData
    {
        public PropertyInfo PropertyStructure { get; set; }
        public PropertyInfo PropertyStructureLastCheck { get; set; }
    }
}
