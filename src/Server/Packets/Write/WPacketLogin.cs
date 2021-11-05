using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;

namespace GameServer.Server.Packets
{
    public class WPacketLogin : IWritable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public ServerVersion ServerVersion { get; set; }
        public Dictionary<ResourceType, uint> ResourceCounts { get; set; }
        public Dictionary<StructureType, uint> StructureCounts { get; set; }

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
        }
    }
}
