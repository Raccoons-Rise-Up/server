using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;

namespace GameServer.Server.Packets
{
    public class WPacketLogin : IWritable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionPatch { get; set; }
        public uint Wood { get; set; }
        public uint Stone { get; set; }
        public uint Wheat { get; set; }
        public uint Gold { get; set; }
        public uint StructureHuts { get; set; }
        public uint StructureWheatFarms { get; set; }
        public List<Structure> Structures { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)LoginOpcode);

            switch (LoginOpcode) 
            {
                case LoginResponseOpcode.VersionMismatch:
                    writer.Write(VersionMajor);
                    writer.Write(VersionMinor);
                    writer.Write(VersionPatch);
                    break;
                case LoginResponseOpcode.LoginSuccessReturningPlayer:
                    writer.Write(Wood);
                    writer.Write(Stone);
                    writer.Write(Wheat);
                    writer.Write(Gold);
                    writer.Write(StructureHuts);
                    writer.Write(StructureWheatFarms);
                    break;
            }

            SendStructureData(ref writer);
        }

        public void SendStructureData(ref PacketWriter writer)
        {
            writer.Write((uint)Structures.Count);
            foreach (var structure in Structures)
            {
                writer.Write((uint)structure.Id);
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
