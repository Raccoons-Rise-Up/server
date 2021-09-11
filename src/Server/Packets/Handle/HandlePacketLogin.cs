using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Logging;
using GameServer.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;

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
                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.InvalidToken
                }), peer, PacketFlags.Reliable);
                return;
            }

            // Check if versions match
            if (data.VersionMajor != ENetServer.ServerVersion.Major || data.VersionMinor != ENetServer.ServerVersion.Minor || data.VersionPatch != ENetServer.ServerVersion.Patch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ENetServer.ServerVersion.Major}.{ENetServer.ServerVersion.Minor}.{ENetServer.ServerVersion.Patch}";

                Logger.Log($"Player '{token.Payload.username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    ServerVersion = ENetServer.ServerVersion
                }), peer, PacketFlags.Reliable);

                return;
            }

            // Check if username exists in database
            var playerUsername = token.Payload.username;
            var player = PlayerManager.GetPlayerConfig(playerUsername);

            // These values will be sent to the client
            WPacketLogin packetData;

            if (player != null)
            {
                // RETURNING PLAYER

                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessReturningPlayer,
                    ResourceCounts = player.ResourceCounts,
                    StructureCounts = player.StructureCounts,
                    ResourceInfoData = ENetServer.ResourceInfoData,
                    StructureInfoData = ENetServer.StructureInfoData
                };

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, player);

                Logger.Log($"Player '{playerUsername}' logged in");
            }
            else
            {
                // NEW PLAYER
                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessNewPlayer,
                    ResourceInfoData = ENetServer.ResourceInfoData,
                    StructureInfoData = ENetServer.StructureInfoData
                };

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, new Player(playerUsername, peer));

                Logger.Log($"User '{playerUsername}' logged in for the first time");
            }

            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData), peer, PacketFlags.Reliable);
        }
    }
}
