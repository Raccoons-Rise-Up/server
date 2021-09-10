using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server.Packets;
using GameServer.Utilities;

namespace GameServer.Server
{
    public abstract class Structure
    {
        public static uint StructureCount { get; private set; }
        public uint Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Dictionary<ResourceType, uint> Cost { get; set; }
        public virtual Dictionary<ResourceType, uint> Production { get; set; }
        public virtual List<TechType> TechRequired { get; set; }

        public Structure() 
        {
            Id = StructureCount++;
            Name = GetType().Name.Replace("Structure", "");
            Name = Utils.AddSpaceBeforeEachCapital(Name);
            Description = "No description was given for this structure.";
            Cost = new();
            Production = new();
            TechRequired = new();
        }
    }
}
