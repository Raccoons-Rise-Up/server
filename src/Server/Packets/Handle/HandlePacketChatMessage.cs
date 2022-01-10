using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Common.Networking.Packet;
using Common.Networking.IO;
using Common.Game;
using ENet;
using GameServer.Logging;
using System.IO;

namespace GameServer.Server.Packets
{
    public class HandlePacketChatMessage : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketChatMessage() => Opcode = ClientPacketOpcode.ChatMessage;

        // Toggleable profanifty options
        public bool useProfanityFilter = false;
        public bool preventCapsCombo = false;

        // read in bad words from file and split them into string array
        private readonly static string blockedStringFile = File.ReadAllText(@"\Blocked-Strings-List");
        private readonly string[] blockedStrings = blockedStringFile.Split("\\n");


        public override void Handle(Event netEvent, ref PacketReader packetReader)
        {
            var data = new RPacketChatMessage();
            data.Read(packetReader);

            var peer = netEvent.Peer;

            var player = ENetServer.Players[peer.ID];

            var message = data.Message;

            // Remove any trailing spaces at start and end
            message = message.Trim();

            private  string[] blockedStringReplacement = { "meowww", "meow", "meeow", "purrr", "meowwww", "pur" };
            
            if(useProfanityFilter){
            foreach (var blockedString in blockedStrings){
                
                Random random = new Random();
                var replacementIndex = random.Next(blockedStringReplacement.Length);

                // if you want to block any combination of caps
                if(preventCapsCombo){
                    message = Regex.Replace(message, blockedString, blockedStringReplacement[replacementIndex], RegexOptions.IgnoreCase);
                }
                else{
                    message = message.Replace(blockedString, blockedStringReplacement[replacementIndex]);
                }
            }
            }
            
            // Add message to channel
            ENetServer.Channels[data.ChannelId].Messages.Add(new UIMessage {
                UserId = peer.ID,
                Message = message
            });

            if (data.ChannelId == (uint)SpecialChannel.Global || data.ChannelId == (uint)SpecialChannel.Game)
            {
                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChatMessage, new WPacketChatMessage
                {
                    UserId = peer.ID,
                    ChannelId = data.ChannelId,
                    Message = message
                }), ENetServer.GetAllPeers());
            }
            else 
            {
                var peers = new List<Peer>();

                foreach (var userId in ENetServer.Channels[data.ChannelId].Users.Keys)
                    if (ENetServer.Players.ContainsKey(userId)) // Only send the chat message to users that are online
                        peers.Add(ENetServer.Players[userId].Peer);

                ENetServer.Send(new ServerPacket((byte)ServerPacketOpcode.ChatMessage, new WPacketChatMessage
                {
                    UserId = peer.ID,
                    ChannelId = data.ChannelId,
                    Message = message
                }), peers);
            }

            Logger.Log($"[{data.ChannelId}] {player.Username}: {message}");
        }
    }
}
