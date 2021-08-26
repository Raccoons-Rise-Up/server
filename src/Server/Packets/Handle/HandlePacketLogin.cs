using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Database;
using GameServer.Logging;

namespace GameServer.Server.Packets
{
    public class HandlePacketLogin : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketLogin() 
        {
            Opcode = ClientPacketOpcode.Login;
        }

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketLogin();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            // Check if versions match
            if (data.VersionMajor != ENetServer.ServerVersionMajor || data.VersionMinor != ENetServer.ServerVersionMinor ||
                data.VersionPatch != ENetServer.ServerVersionPatch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ENetServer.ServerVersionMajor}.{ENetServer.ServerVersionMinor}.{ENetServer.ServerVersionPatch}";

                Logger.Log($"Player '{data.Username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    VersionMajor = ENetServer.ServerVersionMajor,
                    VersionMinor = ENetServer.ServerVersionMinor,
                    VersionPatch = ENetServer.ServerVersionPatch,
                };

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData);
                ENetServer.Send(packet, peer, PacketFlags.Reliable);

                return;
            }

            // Check if username exists in database
            using var db = new DatabaseContext();

            var dbPlayer = db.Players.ToList().Find(x => x.Username == data.Username);

            // These values will be sent to the client
            var playerValues = new PlayerValues();

            if (dbPlayer != null)
            {
                // RETURNING PLAYER

                playerValues.Gold = dbPlayer.Gold;
                playerValues.StructureHuts = dbPlayer.StructureHut;

                // Add the player to the list of players currently on the server
                var player = new Player
                {
                    Gold = dbPlayer.Gold,
                    StructureHut = dbPlayer.StructureHut,
                    LastCheckStructureHut = dbPlayer.LastCheckStructureHut,
                    LastSeen = DateTime.Now,
                    Username = dbPlayer.Username,
                    Peer = peer,
                    Ip = peer.IP
                };

                ENetServer.Players.Add(player);

                Logger.Log($"Player '{data.Username}' logged in");
            }
            else
            {
                // NEW PLAYER

                playerValues.Gold = StartingValues.Gold;
                playerValues.StructureHuts = StartingValues.StructureHuts;

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(new Player
                {
                    Peer = peer,
                    Username = data.Username,
                    Gold = StartingValues.Gold,
                    LastSeen = DateTime.Now,
                    Ip = peer.IP
                });

                Logger.Log($"User '{data.Username}' logged in for the first time");
            }

            {
                var packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccess,
                    Gold = playerValues.Gold,
                    StructureHuts = playerValues.StructureHuts
                };

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData);
                ENetServer.Send(packet, peer, PacketFlags.Reliable);
            }
        }
    }
}
