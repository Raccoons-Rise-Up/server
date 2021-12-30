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
                }), peer);
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
                }), peer);

                return;
            }

            // Check if username exists in database
            var playerUsername = token.Payload.username;

            // Check if username is in the player banlist
            var bannedPlayers = FileManager.ReadConfig<List<BannedPlayer>>("banned_players");
            var bannedPlayer = bannedPlayers.Find(x => x.Name == playerUsername);

            if (bannedPlayer != null) 
            {
                // Player is banned, disconnect them immediately 
                netEvent.Peer.DisconnectNow((uint)DisconnectOpcode.Banned);
                Logger.Log($"Player '{bannedPlayer.Name}' tried to join but is banned");
                return;
            }

            // Check if a player with this username is logged in already
            foreach (var p in ENetServer.Players)
            {
                if (p.Value.Username.Equals(playerUsername)) 
                {
                    netEvent.Peer.DisconnectNow((uint)DisconnectOpcode.PlayerWithUsernameExistsOnServerAlready);
                    return;
                }
            }

            // These values will be sent to the client
            WPacketLogin packetData;

            var player = Player.GetPlayerConfig(playerUsername);

            if (player != null)
            {
                // RETURNING PLAYER

                player.Peer = netEvent.Peer;
                player.AddResourcesGeneratedFromStructures();

                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessReturningPlayer,
                    ResourceCounts = player.ResourceCounts.ToDictionary(x => x.Key, x => (uint)x.Value),
                    StructureCounts = player.StructureCounts,
                    PlayerId = player.Peer.ID,
                    PlayerName = playerUsername
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
                    PlayerId = netEvent.Peer.ID,
                    PlayerName = playerUsername
                };

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, new Player(playerUsername, peer));

                Logger.Log($"User '{playerUsername}' logged in for the first time");
            }

            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData), peer);

            // Tell the joining client how many players are on the server
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.PlayerList, new WPacketPlayerList()), peer);

            // Add the user to the global channel
            ENetServer.Channels[(uint)SpecialChannel.Global].Users.Add(peer.ID, playerUsername);

            // Tell the joining client how many channels they should know about
            var channelsToSend = new Dictionary<uint, UIChannel>();
            foreach (var pair in ENetServer.Channels) 
                foreach (var user in pair.Value.Users) 
                    if (user.Key == peer.ID) // If the peer is a participant of this channel add it
                    {
                        channelsToSend.Add(pair.Key, pair.Value);
                        break;
                    }

            // No need to add Global channel as Game channel has no users in it, it is a special channel
            channelsToSend.Add((uint)SpecialChannel.Game, ENetServer.Channels[(uint)SpecialChannel.Game]);

            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChannelList, new WPacketChannelList { 
                Channels = channelsToSend
            }), peer);


            // Tell all other clients that this player has joined
            ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.PlayerJoinLeave, new WPacketPlayerJoinLeave {
                JoinLeaveOpcode = JoinLeaveOpcode.Join,
                PlayerId = peer.ID,
                PlayerName = playerUsername
            }), ENetServer.GetOtherPeers(peer));
        }
    }
}
