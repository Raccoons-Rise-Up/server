using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Server.Packets;

namespace GameServer.Server
{
    public abstract class Structure
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Dictionary<ResourceType, uint> Cost { get; set; }
        public virtual Dictionary<ResourceType, uint> Production { get; set; }
        public virtual List<TechType> TechRequired { get; set; }

        public Structure() 
        {
            Name = GetType().Name.Replace("Structure", "");
            Description = "No description was given for this structure.";
            Cost = new();
            Production = new();
            TechRequired = new();
        }
    }
}
