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
using GameServer.Server.Security;
using GameServer.Server;
using GameServer.Utilities;
using System.Net.Http;

namespace GameServer.Server.Packets
{
    public class HandlePacketLogin : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketLogin() 
        {
            Opcode = ClientPacketOpcode.Login;
        }

        static async Task PostRequest(RPacketLogin data) 
        {
            try
            {
                var values = new Dictionary<string, string>
                {
                    { "username", data.Username },
                    { "password", data.PasswordHash }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await ENetServer.WebClient.PostAsync("http://localhost:4000/api/login", content);
                Thread.CurrentThread.Name = "SERVER";

                var responseString = await response.Content.ReadAsStringAsync();

                var responseObj = Utils.ReadJSONString<WebLoginResponse>(responseString);

                var opcode = (WebLoginResponseOpcode)responseObj.opcode;

                if (opcode == WebLoginResponseOpcode.AccountDoesNotExist) 
                {
                    Logger.Log("account does not exist");
                }

                if (opcode == WebLoginResponseOpcode.InvalidUsernameOrPassword) 
                {
                    Logger.Log("invalid username or password");
                }

                if (opcode == WebLoginResponseOpcode.AccountDoesNotExist) 
                {
                    Logger.Log("account does not exist");
                }

                if (opcode == WebLoginResponseOpcode.PasswordsDoNotMatch) 
                {
                    Logger.Log("passwords do not match");
                }

                if (opcode == WebLoginResponseOpcode.LoginSuccess) 
                {
                    Logger.Log("login success");
                }
            }
            catch (HttpRequestException e) 
            {
                Thread.CurrentThread.Name = "SERVER";
                Logger.Log(e.Message);
            }
        }

        static async Task GetRequest()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await ENetServer.WebClient.GetAsync("http://localhost:4000/api/");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async override void Handle(Event netEvent, PacketReader packetReader)
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

            // Check if username / password match up with web server
            try
            {
                await PostRequest(data);
            }
            catch (TaskCanceledException e) 
            {
                Logger.Log(e.Message);
            }
            
            /*var values = new Dictionary<string, string>
            {
                { "username", data.Username },
                { "password", data.PasswordHash }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await ENetServer.WebClient.PostAsync("http://localhost:4000/api/login", content).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            response.Dispose();

            Logger.Log(responseString);*/

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

            packetReader.Dispose();
        }
    }
}
