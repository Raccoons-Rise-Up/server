using Common.Netcode;
using ENet;
using GameServer.Console;
using GameServer.Utils;
using System.Collections.Generic;
using System;
using System.Linq;
using GameServer.Server.Game;

namespace GameServer.Server.Packets
{
    public class HandlePacketLogin : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketLogin() => Opcode = ClientPacketOpcode.Login;

        public override void Handle(Peer peer, PacketReader packetReader)
        {
            var data = new RPacketLogin();
            data.Read(packetReader);

            // Check JWT
            var token = new JsonWebToken(data.JsonWebToken);
            if (token.IsValid.Error != JsonWebToken.TokenValidateError.Ok)
            {
                ENetServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.InvalidToken
                }, peer));
                return;
            }

            // Check if versions match
            if (data.VersionMajor != ENetServer.Version.Major || data.VersionMinor != ENetServer.Version.Minor || data.VersionPatch != ENetServer.Version.Patch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ENetServer.Version.Major}.{ENetServer.Version.Minor}.{ENetServer.Version.Patch}";

                Logger.Log($"Player '{token.Payload.username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                ENetServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    ServerVersion = ENetServer.Version
                }, peer));

                return;
            }

            // Check if username exists in database
            var playerUsername = token.Payload.username;

            // These values will be sent to the client
            WPacketLogin packetData;

            var player = PlayerUtils.GetConfig(playerUsername);

            if (player != null)
            {
                // RETURNING PLAYER

                player.Peer = peer;
                //player.AddResourcesGeneratedFromStructures();

                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessReturningPlayer
                };

                //player.InGame = true;

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, player);

                Logger.Log($"Player '{playerUsername}' logged in");
            }
            else
            {
                // NEW PLAYER
                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessNewPlayer
                };

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, new ServerPlayer(peer, playerUsername));

                Logger.Log($"User '{playerUsername}' logged in for the first time");
            }

            ENetServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData, peer));
        }
    }
}
