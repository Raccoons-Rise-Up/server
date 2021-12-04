using Common.Networking.Packet;
using Common.Networking.IO;
using ENet;
using GameServer.Logging;
using GameServer.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using Common.Game;

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

            // Consider the following scenario:
            // 1. A new resource gets added to the game
            // 2. But the current player config did not get these updated changes
            // 3. That's what this code does, it updates the player config to include these new changes
            var resourceCountTypes = Enum.GetValues(typeof(ResourceType));

            if (player.ResourceCounts.Count < resourceCountTypes.Length)
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    if (!player.ResourceCounts.ContainsKey(type))
                    {
                        player.ResourceCounts.Add(type, 0);
                    }
                }
            }

            // Consider the following scenario:
            // 1. A new structure gets added to the game
            // 2. But the current player config did not get these updated changes
            // 3. That's what this code does, it updates the player config to include these new changes
            var structureCountTypes = Enum.GetValues(typeof(StructureType));

            if (player.StructureCounts.Count < structureCountTypes.Length) 
            {
                foreach (StructureType type in Enum.GetValues(typeof(StructureType))) 
                {
                    if (!player.StructureCounts.ContainsKey(type)) 
                    {
                        player.StructureCounts.Add(type, 0);
                    }
                }
            }

            // These values will be sent to the client
            WPacketLogin packetData;

            if (player != null)
            {
                // RETURNING PLAYER

                player.Peer = netEvent.Peer;
                player.AddResourcesGeneratedFromStructures();

                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessReturningPlayer,
                    ResourceCounts = player.ResourceCounts.ToDictionary(x => x.Key, x => (uint)x.Value),
                    StructureCounts = player.StructureCounts
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
                    LoginOpcode = LoginResponseOpcode.LoginSuccessNewPlayer
                };

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, new Player(playerUsername, peer));

                Logger.Log($"User '{playerUsername}' logged in for the first time");
            }

            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData), peer, PacketFlags.Reliable);
        }
    }
}
