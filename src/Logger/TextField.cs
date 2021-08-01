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

        /// <summary>
        /// Redraw the text field
        /// </summary>
        public void Redraw()
        {
            var prevCursorLeft = Console.CursorLeft;
            Clear(false);

            Console.WriteLine(m_Input);
            Console.CursorLeft = prevCursorLeft;
        }

        /// <summary>
        /// Move the text field down
        /// </summary>
        public void MoveDown()
        {
            var prevCursorLeft = Console.CursorLeft;
            Clear(false);
            Console.CursorTop++;

            Console.WriteLine(m_Input);

            Console.CursorLeft = prevCursorLeft;
        }

        /// <summary>
        /// Clear the text field display and optionally clear the input
        /// </summary>
        public void Clear(bool clearInput)
        {
            if (clearInput)
                m_Input = "";

            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop--;
        }
    }
}
