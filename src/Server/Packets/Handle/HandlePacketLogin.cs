using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Database;
using GameServer.Logging;
using GameServer.Utilities;
using GameServer.Server.Security;
using GameServer.Server;
using System.Net.Http;

namespace GameServer.Server.Packets
{
    public class HandlePacketLogin : HandlePacket
    {
        public override ClientOpcode Opcode { get; set; }

        public HandlePacketLogin() 
        {
            Opcode = ClientOpcode.Login;
        }

        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketLogin();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            // Check JWT
            var token = new JsonWebToken(data.JsonWebToken);
            if (token.IsValid.Error != JsonWebToken.TokenValidateError.Ok) 
            {
                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.InvalidToken
                });
                ENetServer.Send(packet, peer, PacketFlags.Reliable);
                return;
            }

            // Check if versions match
            if (data.VersionMajor != ENetServer.ServerVersion.Major || data.VersionMinor != ENetServer.ServerVersion.Minor ||
                data.VersionPatch != ENetServer.ServerVersion.Patch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ENetServer.ServerVersion.Major}.{ENetServer.ServerVersion.Minor}.{ENetServer.ServerVersion.Patch}";

                Logger.Log($"Player '{token.Payload.username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    VersionMajor = ENetServer.ServerVersion.Major,
                    VersionMinor = ENetServer.ServerVersion.Minor,
                    VersionPatch = ENetServer.ServerVersion.Patch,
                });
                ENetServer.Send(packet, peer, PacketFlags.Reliable);

                return;
            }

            // Check if username exists in database
            using var db = new DatabaseContext();

            var dbPlayer = db.Players.FirstOrDefault(x => x.Username == token.Payload.username);

            // These values will be sent to the client
            WPacketLogin packetData;

            if (dbPlayer != null)
            {
                // RETURNING PLAYER

                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessReturningPlayer,
                    Wood = dbPlayer.ResourceWood,
                    Stone = dbPlayer.ResourceStone,
                    Gold = dbPlayer.ResourceGold,
                    Wheat = dbPlayer.ResourceWheat,
                    StructureHuts = dbPlayer.StructureHut,
                    StructureWheatFarms = dbPlayer.StructureWheatFarm,
                    Structures = ENetServer.Structures.Values.ToList()
                };

                // Add the player to the list of players currently on the server
                var player = new Player
                {
                    ResourceGold = dbPlayer.ResourceGold,
                    ResourceWood = 101,
                    ResourceWheat = 50,
                    StructureHut = dbPlayer.StructureHut,
                    LastCheckStructureHut = dbPlayer.LastCheckStructureHut,
                    LastCheckStructureWheatFarm = dbPlayer.LastCheckStructureWheatFarm,
                    LastSeen = DateTime.Now,
                    Username = dbPlayer.Username,
                    Peer = peer,
                    Ip = peer.IP
                };

                ENetServer.Players.Add(player);

                Logger.Log($"Player '{token.Payload.username}' logged in");
            }
            else
            {
                // NEW PLAYER
                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessNewPlayer,
                    Structures = ENetServer.Structures.Values.ToList()
                };


                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(new Player
                {
                    Peer = peer,
                    Username = token.Payload.username,
                    LastSeen = DateTime.Now,
                    Ip = peer.IP
                });

                Logger.Log($"User '{token.Payload.username}' logged in for the first time");
            }

            {
                

                var packet = new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData);
                ENetServer.Send(packet, peer, PacketFlags.Reliable);
            }
        }
    }
}
