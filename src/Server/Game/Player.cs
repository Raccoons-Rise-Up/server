using System;
using System.Reflection;
using System.Collections.Generic;
using GameServer.Database;
using GameServer.Logging;
using ENet;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Server.Packets;
using GameServer.Utilities;

namespace GameServer.Server
{
    public class Player : ModelPlayer
    {
        public Peer Peer { get; set; }
        private Dictionary<string, PropertyInfo> PropertyResources { get; set; }
        private Dictionary<string, StructurePropertyData> PropertyStructures { get; set; }

        public Player()
        {
            PropertyResources = new();
            PropertyStructures = new();

            var properties = typeof(ModelPlayer).GetProperties();

            foreach (var prop in properties.Where(x => x.Name.StartsWith("Resource")))
                PropertyResources.Add(prop.Name.Replace("Resource", ""), prop);

            var propertiesStructure = new Dictionary<string, PropertyInfo>();
            var propertiesStructureLastCheck = new Dictionary<string, PropertyInfo>();

            foreach (var prop in properties.Where(x => x.Name.StartsWith("Structure"))) 
                propertiesStructure.Add(prop.Name.Replace("Structure", ""), prop);

            foreach (var prop in properties.Where(x => x.Name.StartsWith("LastCheckStructure"))) 
                propertiesStructureLastCheck.Add(prop.Name.Replace("LastCheckStructure", ""), prop);

            foreach (var prop in propertiesStructure)
            {
                PropertyStructures.Add(prop.Key, new StructurePropertyData
                {
                    PropertyStructure = prop.Value,
                    PropertyStructureLastCheck = propertiesStructureLastCheck[prop.Key]
                });
            }
        }

        public PurchaseResult TryPurchase(Structure structure) 
        {
            AddResourcesGeneratedFromStructures();

            var lackingResources = GetLackingResources(structure);
            if (lackingResources.Count > 0)
                return new PurchaseResult 
                {
                    Result = PurchaseEnumResult.LackingResources,
                    Resources = lackingResources
                };

            var newPlayerResources = new Dictionary<ResourceType, uint>();

            foreach (var resource in PropertyResources) 
            {
                var playerResource = (uint)resource.Value.GetValue(this);
                if (structure.Cost.TryGetValue((ResourceType)Enum.Parse(typeof(ResourceType), resource.Key), out uint resourceCost)) 
                {
                    var remaining = playerResource - resourceCost;
                    resource.Value.SetValue(this, remaining);
                    newPlayerResources.Add((ResourceType)Enum.Parse(typeof(ResourceType), resource.Key), remaining);
                }
            }

            PropertyStructures[structure.Name].PropertyStructure.SetValue(this, (uint)PropertyStructures[structure.Name].PropertyStructure.GetValue(this) + 1);

            return new PurchaseResult 
            {
                Result = PurchaseEnumResult.Success,
                Resources = newPlayerResources
            };
        }

        public Dictionary<ResourceType, uint> GetLackingResources(Structure structure) 
        {
            var lackingResources = new Dictionary<ResourceType, uint>();
            foreach (var resource in PropertyResources) 
            {
                var playerResource = (uint)resource.Value.GetValue(this);
                if (structure.Cost.TryGetValue((ResourceType)Enum.Parse(typeof(ResourceType), resource.Key), out uint resourceCost))
                    if (playerResource < resourceCost) 
                    {
                        lackingResources.Add((ResourceType)Enum.Parse(typeof(ResourceType), resource.Key), resourceCost - playerResource);
                    }
            }

            return lackingResources;
        }

        public void AddResourcesGeneratedFromStructures()
        {
            foreach (var lastCheck in PropertyStructures)
            {
                var diff = DateTime.Now - (DateTime)lastCheck.Value.PropertyStructureLastCheck.GetValue(this);
                var structureName = lastCheck.Key;

                // expensive and ugly line of code
                var structure = ENetServer.Structures[Utils.AddSpaceBeforeEachCapital(structureName)];

                var production = structure.Production; // Structure production

                // Some structures don't have any production
                if (production == null)
                    continue;

                foreach (var prod in production)
                {
                    // structure count * structure production * time elapsed
                    var resourceGenerated = (uint)((uint)PropertyStructures[structureName].PropertyStructure.GetValue(this) * prod.Value * diff.TotalSeconds);

                    PropertyResources[prod.Key.ToString()].SetValue(this, (uint)PropertyResources[prod.Key.ToString()].GetValue(this) + resourceGenerated);
                }

                PropertyStructures[structureName].PropertyStructureLastCheck.SetValue(this, DateTime.Now);
            }
        }

        public void ResetValues() 
        {
            LastSeen = DateTime.Now;

            foreach (var resource in PropertyResources.Values)
                resource.SetValue(this, (uint)0);

            foreach (var structure in PropertyStructures.Values)
                structure.PropertyStructure.SetValue(this, (uint)0);

            foreach (var lastCheck in PropertyStructures.Values)
                lastCheck.PropertyStructureLastCheck.SetValue(this, DateTime.Now);
        }
    }

    public struct PurchaseResult 
    {
        public Dictionary<ResourceType, uint> Resources { get; set; }
        public PurchaseEnumResult Result { get; set; }
    }

    public enum PurchaseEnumResult 
    {
        LackingResources,
        Success
    }

    public struct StructurePropertyData
    {
        public PropertyInfo PropertyStructure { get; set; }
        public PropertyInfo PropertyStructureLastCheck { get; set; }
    }
}
