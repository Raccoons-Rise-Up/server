using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ENet;
using Common.Game;

namespace GameServer.Server
{
    public class Player
    {
        [JsonIgnore] public Peer Peer { get; set; }
        public string Username { get; set; }
        public DateTime LastSeen { get; set; }
        public Dictionary<ResourceType, uint> ResourceCounts { get; set; }
        public Dictionary<StructureType, uint> StructureCounts { get; set; }
        public Dictionary<StructureType, DateTime> StructuresLastSeen { get; set; }

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

            StructuresLastSeen = new();
        }

        public PurchaseResult TryPurchase(StructureInfo structure)
        {
            //AddResourcesGeneratedFromStructures();

            var lackingResources = GetLackingResources(structure);
            if (lackingResources.Count > 0)
                return new PurchaseResult
                {
                    Result = PurchaseEnumResult.LackingResources,
                    Resources = lackingResources
                };

            var newPlayerResources = new Dictionary<ResourceType, uint>();

            foreach (var resource in ResourceCounts)
            {
                var playerResourceKey = resource.Key;
                var playerResourceAmount = resource.Value;

                if (structure.Cost.TryGetValue(resource.Key, out uint resourceCost))
                {
                    var remaining = playerResourceAmount - resourceCost;
                    ResourceCounts[playerResourceKey] = remaining;
                    newPlayerResources.Add(playerResourceKey, remaining);
                }
            }

            StructureCounts[structure.Type] += 1;

            return new PurchaseResult
            {
                Result = PurchaseEnumResult.Success,
                Resources = newPlayerResources
            };
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
                        lackingResources.Add(playerResourceKey, structureResourceValue - playerResourceAmount);
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
