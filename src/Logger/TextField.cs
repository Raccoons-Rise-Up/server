using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class TextField
    {
        public string m_Input;
        public int m_Column;
        public int m_Row;

        public void Redraw()
        {
            var prevCursorLeft = Console.CursorLeft;
            Clear();

            Console.WriteLine(m_Input);
            Console.CursorLeft = prevCursorLeft;
        }

        public void MoveDown()
        {
            Clear();

            Console.Write(m_Input);
        }

        /// <summary>
        /// Clear the text field input
        /// </summary>
        public void Clear()
        {
            m_Input = "";

            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop--;
        }
    }
}
