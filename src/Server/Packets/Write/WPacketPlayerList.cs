using System.Collections.Generic;
using Common.Networking.IO;
using Common.Networking.Message;
using Common.Networking.Packet;
using Common.Game;
using GameServer.Logging;

namespace GameServer.Server.Packets
{
    public class WPacketPlayerList : IWritable
    {
        public void Write(PacketWriter writer)
        {
            foreach (var p in ENetServer.Players)
                Logger.Log($"{p.Key}: {p.Value}");

            writer.Write((byte)ENetServer.Players.Count);
            foreach (var p in ENetServer.Players) 
            {
                writer.Write(p.Key);
                writer.Write(p.Value.Username);
            }
        }
    }
}
