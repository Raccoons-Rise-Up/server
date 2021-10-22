using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ENet;
using Common.Game;
using GameServer.Logging;

namespace GameServer.Server
{
    public class Player
    {
        [JsonIgnore] public Peer Peer { get; set; }
        public string Username { get; set; }
        public DateTime LastSeen { get; set; }
        public Dictionary<ResourceType, float> ResourceCounts { get; set; }
        public Dictionary<StructureType, uint> StructureCounts { get; set; }
        public DateTime StructuresLastChecked { get; set; }

        public Player(string username, Peer peer) 
        {
            Peer = peer;
            Username = username;
            LastSeen = DateTime.Now;

            ResourceCounts = new();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                ResourceCounts.Add(type, 0);

            StructureCounts = new();
            foreach (StructureType type in Enum.GetValues(typeof(StructureType)))
                StructureCounts.Add(type, 0);

            StructuresLastChecked = new();
        }

        public PurchaseResult TryPurchase(StructureInfo structure)
        {
            AddResourcesGeneratedFromStructures();

            // Return the resources that the player is lacking in order to make this purchase
            var lackingResources = GetLackingResources(structure);
            if (lackingResources.Count > 0)
                return new PurchaseResult
                {
                    Result = PurchaseEnumResult.LackingResources
                };

            // The player has enough to purchase this
            var newPlayerResources = new Dictionary<ResourceType, uint>();

            foreach (var resource in ResourceCounts)
            {
                if (structure.Cost.TryGetValue(resource.Key, out uint resourceCost))
                {
                    ResourceCounts[resource.Key] -= resourceCost;
                    newPlayerResources.Add(resource.Key, (uint)resource.Value);
                }
            }

            // Need to ensure all the resources are being updated (not just for cost resources but for production resources too)
            foreach (var resourceKey in structure.Production.Keys) 
            {
                if (!newPlayerResources.ContainsKey(resourceKey)) 
                    newPlayerResources.Add(resourceKey, (uint)ResourceCounts[resourceKey]);
            }

            StructureCounts[structure.Type] += 1;

            // Return the now updated player resource values based on this new purchase
            return new PurchaseResult
            {
                Result = PurchaseEnumResult.Success,
                Resources = newPlayerResources
            };
        }

        public void AddResourcesGeneratedFromStructures() 
        {
            foreach (var structureCount in StructureCounts) 
            {
                var structureData = ENetServer.StructureInfoData[structureCount.Key];

                foreach (var prod in structureData.Production) 
                {
                    var timeDiff = DateTime.Now - StructuresLastChecked;
                    var amountGenerated = prod.Value * structureCount.Value * (uint)timeDiff.TotalSeconds;

                    ResourceCounts[prod.Key] += amountGenerated;
                }
            }

            StructuresLastChecked = DateTime.Now;
        }

        public Dictionary<ResourceType, uint> GetLackingResources(StructureInfo structure)
        {
            var lackingResources = new Dictionary<ResourceType, uint>();
            foreach (var resource in ResourceCounts)
            {
                var playerResourceKey = resource.Key;
                var playerResourceAmount = resource.Value;

                if (structure.Cost.TryGetValue(playerResourceKey, out uint structureResourceValue)) 
                {
                    if (playerResourceAmount < structureResourceValue)
                    {
                        lackingResources.Add(playerResourceKey, (uint)(structureResourceValue - playerResourceAmount));
                    }
                }
            }

            return lackingResources;
        }

        public override string ToString()
        {
            return Username;
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
