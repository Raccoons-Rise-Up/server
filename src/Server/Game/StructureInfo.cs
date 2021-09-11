using System;
using System.Collections.Generic;
using GameServer.Utilities;

namespace GameServer.Server
{
    public abstract class StructureInfo
    {
        public virtual StructureType Type { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Dictionary<ResourceType, uint> Cost { get; set; }
        public virtual Dictionary<ResourceType, uint> Production { get; set; }
        public virtual List<TechType> TechRequired { get; set; }

        public StructureInfo() 
        {
            var rawName = GetType().Name.Replace(typeof(StructureInfo).Name, "");
            
            Name = Utils.AddSpaceBeforeEachCapital(rawName);
            Type = (StructureType)Enum.Parse(typeof(StructureType), rawName);
            Description = "No description was given for this structure.";
            Cost = new();
            Production = new();
            TechRequired = new();
        }
    }

    public enum StructureType
    {
        Hut,
        WheatFarm
    }

    public enum TechType
    {

    }
}
