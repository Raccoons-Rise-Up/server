using System;

namespace GameServer.Logging
{
    public class LoggerMessage
    {
        public readonly string text;

        public LoggerMessage(string text) 
        {
            this.text = text;
        }

        public int GetLines() 
        {
            var lines = 0;
            lines += (int)Math.Ceiling((double)text.Length / Console.WindowWidth);
            lines += text.Split('\n').Length - 1;
            return lines;
        }
    }
}
