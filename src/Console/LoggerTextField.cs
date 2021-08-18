using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logging
{
    public class LoggerTextField
    {
        public string input;
        public int column;
        public int row;

        /// <summary>
        /// Redraw the text field
        /// </summary>
        public void Redraw()
        {
            var prevCursorLeft = Console.CursorLeft;
            Clear(false);

            Console.WriteLine(input);
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

            Console.WriteLine(input);

            Console.CursorLeft = prevCursorLeft;
        }

        /// <summary>
        /// Clear the text field display and optionally clear the input
        /// </summary>
        public void Clear(bool clearInput)
        {
            if (clearInput)
                input = "";

            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop--;
        }
    }
}
