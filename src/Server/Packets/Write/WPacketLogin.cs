using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketLogin : IWritable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public ServerVersion ServerVersion { get; set; }
        public Dictionary<ResourceType, uint> ResourceCounts { get; set; }
        public Dictionary<StructureType, uint> StructureCounts { get; set; }
        public Dictionary<ResourceType, ResourceInfo> ResourceInfoData { get; set; }
        public Dictionary<StructureType, StructureInfo> StructureInfoData { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)LoginOpcode);

            switch (LoginOpcode) 
            {
                case LoginResponseOpcode.VersionMismatch:
                    writer.Write((byte)ServerVersion.Major);
                    writer.Write((byte)ServerVersion.Minor);
                    writer.Write((byte)ServerVersion.Patch);
                    break;
                case LoginResponseOpcode.LoginSuccessReturningPlayer:
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
                    break;
            }

            SendResourceInfo(ref writer);
            SendStructureData(ref writer);
        }

        private void SendResourceInfo(ref PacketWriter writer) 
        {
            writer.Write((ushort)ResourceInfoData.Count);
            foreach (var keyValuePair in ResourceInfoData) 
            {
                var resource = keyValuePair.Value;
                writer.Write((ushort)keyValuePair.Key);
                writer.Write((string)resource.Name);
                writer.Write((string)resource.Description);
            }
        }

        private void SendStructureData(ref PacketWriter writer)
        {
            writer.Write((ushort)StructureInfoData.Count);
            foreach (var keyValuePair in StructureInfoData)
            {
                var structure = keyValuePair.Value;
                writer.Write((ushort)keyValuePair.Key);
                writer.Write((string)structure.Name);
                writer.Write((string)structure.Description);
                writer.Write((byte)structure.Cost.Count); // Assuming a single structure will not cost more than 255 resource types
                foreach (var resource in structure.Cost)
                {
                    writer.Write((ushort)resource.Key);
                    writer.Write(resource.Value);
                }
                writer.Write((byte)structure.Production.Count); // Assuming a single structure will not produce more than 255 resource types
                foreach (var resource in structure.Production)
                {
                    writer.Write((ushort)resource.Key);
                    writer.Write(resource.Value);
                }
                writer.Write((byte)structure.TechRequired.Count); // Assuming a single structure will not have more than 255 tech requirements
                foreach (var tech in structure.TechRequired)
                {
                    writer.Write((ushort)tech);
                }
            }
        }
    }
}
