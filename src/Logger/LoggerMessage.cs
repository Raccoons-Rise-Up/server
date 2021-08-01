using System;

namespace GameServer
{
    public class LoggerMessage
    {
        public readonly string m_Message;

        public LoggerMessage(string message) 
        {
            m_Message = message;
        }

        public int GetLines() 
        {
            var lines = 0;
            lines += (int)Math.Ceiling((double)m_Message.Length / Console.WindowWidth);
            lines += m_Message.Split('\n').Length - 1;
            return lines;
        }
    }
}
