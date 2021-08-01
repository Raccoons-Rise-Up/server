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
            Console.CursorTop++;

            Console.WriteLine(m_Input);
            Console.CursorLeft = prevCursorLeft;
        }

        public void MoveDown()
        {
            Clear();

            Console.CursorTop += 2;
            Console.Write(m_Input);
        }

        public void Clear()
        {
            // Clear the text input field
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop -= 2;
        }
    }
}
