using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace GameServer.Server.Packets
{
    // ================================== Sizes ==================================
    // sbyte   -128 to 127                                                   SByte
    // byte       0 to 255                                                   Byte
    // short   -32,768 to 32,767                                             Int16
    // ushort  0 to 65,535                                                   UInt16
    // int     -2,147,483,648 to 2,147,483,647                               Int32
    // uint    0 to 4,294,967,295                                            UInt32
    // long    -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807       Int64
    // ulong   0 to 18,446,744,073,709,551,615                               UInt64

    public class RPacketPurchaseItem : IReadable
    {
        public ClientPacketType id;
        public uint itemId;

        public void Read(PacketReader reader)
        {
            id = (ClientPacketType)reader.ReadByte();
            itemId = reader.ReadUInt16();
        }
    }
}
