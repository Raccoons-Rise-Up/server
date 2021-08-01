using System;

namespace GameServer.Logging
{
    public class LoggerMessage
    {
        public readonly string m_Text;

        public LoggerMessage(string text) 
        {
            m_Text = text;
        }

        public int GetLines() 
        {
            var lines = 0;
            lines += (int)Math.Ceiling((double)m_Text.Length / Console.WindowWidth);
            lines += m_Text.Split('\n').Length - 1;
            return lines;
        }
    }
}
