using System;
using System.Reflection;
using System.Collections.Generic;
using GameServer.Database;
using GameServer.Logging;
using ENet;
using System.Linq;
using System.Threading.Tasks;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public class Player : ModelPlayer
    {
        public Peer Peer { get; set; }
        private Dictionary<string, PropertyInfo> PropertyResources { get; set; }
        private Dictionary<string, PropertyInfo> PropertyStructures { get; set; }
        private Dictionary<string, PropertyInfo> PropertyStructureLastChecks { get; set; }

        public Player()
        {
            PropertyResources = new();
            PropertyStructures = new();
            PropertyStructureLastChecks = new();

            var properties = typeof(ModelPlayer).GetProperties();

            foreach (var prop in properties.Where(x => x.Name.StartsWith("Resource")))
                PropertyResources.Add(prop.Name.Replace("Resource", ""), prop);

            foreach (var prop in properties.Where(x => x.Name.StartsWith("Structure")))
                PropertyStructures.Add(prop.Name.Replace("Structure", ""), prop);

            foreach (var prop in properties.Where(x => x.Name.StartsWith("LastCheckStructure")))
                PropertyStructureLastChecks.Add(prop.Name.Replace("LastCheckStructure", ""), prop);
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
                if (structure.Cost.TryGetValue(resource.Key, out uint resourceCost)) 
                {
                    var remaining = playerResource - resourceCost;
                    resource.Value.SetValue(this, remaining);
                    newPlayerResources.Add((ResourceType)Enum.Parse(typeof(ResourceType), resource.Key), remaining);
                }
            }

            PropertyStructures[structure.Name].SetValue(this, (uint)PropertyStructures[structure.Name].GetValue(this) + 1);

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
                if (structure.Cost.TryGetValue(resource.Key, out uint resourceCost))
                    if (playerResource < resourceCost) 
                    {
                        lackingResources.Add((ResourceType)Enum.Parse(typeof(ResourceType), resource.Key), resourceCost - playerResource);
                    }
            }

            return lackingResources;
        }

        public void AddResourcesGeneratedFromStructures()
        {
            foreach (var lastCheck in PropertyStructureLastChecks)
            {
                var diff = DateTime.Now - (DateTime)lastCheck.Value.GetValue(this);
                var structureName = lastCheck.Key;

                var production = ENetServer.Structures[(StructureType)Enum.Parse(typeof(StructureType), structureName)].Production; // Structure production

                // Some structures don't have any production
                if (production == null)
                    continue;

                foreach (var prod in production)
                {
                    // structure count * structure production * time elapsed
                    var resourceGenerated = (uint)((uint)PropertyStructures[structureName].GetValue(this) * prod.Value * diff.TotalSeconds);

                    PropertyResources[prod.Key].SetValue(this, (uint)PropertyResources[prod.Key].GetValue(this) + resourceGenerated);
                }

                PropertyStructureLastChecks[structureName].SetValue(this, DateTime.Now);
            }
        }

        public void ResetValues() 
        {
            LastSeen = DateTime.Now;

            foreach (var resource in PropertyResources.Values)
                resource.SetValue(this, (uint)0);

            foreach (var structure in PropertyStructures.Values)
                structure.SetValue(this, (uint)0);

            foreach (var lastCheck in PropertyStructureLastChecks.Values)
                lastCheck.SetValue(this, DateTime.Now);
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
}
