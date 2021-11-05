using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketPlayerData : IWritable
    {
        public Dictionary<ResourceType, uint> ResourceCounts { get; set; }
        public Dictionary<StructureType, uint> StructureCounts { get; set; }

        public void Write(PacketWriter writer)
        {
            // Resource counts
            writer.Write((ushort)ResourceCounts.Count);
            foreach (var resourceCount in ResourceCounts)
            {
                writer.Write((ushort)resourceCount.Key);
                writer.Write((uint)resourceCount.Value);
            }

            // Structure counts
            writer.Write((ushort)StructureCounts.Count);
            foreach (var structureCount in StructureCounts)
            {
                writer.Write((ushort)structureCount.Key);
                writer.Write((uint)structureCount.Value);
            }
        }
    }
}
