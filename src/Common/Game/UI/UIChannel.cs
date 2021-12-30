using System.Collections.Generic;

namespace Common.Game
{
    public class UIChannel
    {
        public string ChannelName { get; set; }
        public Dictionary<uint, string> Users = new Dictionary<uint, string>();
        public List<UIMessage> Messages = new List<UIMessage>();
        public uint CreatorId { get; set; }

#if CLIENT
        public Godot.Button Button { get; set; }
#endif
    }

    public struct UIMessage 
    {
        public uint UserId { get; set; }
        public string Message { get; set; }
        public bool Special { get; set; }
    }

    public enum SpecialChannel
    {
        Global,
        Game
    }
}