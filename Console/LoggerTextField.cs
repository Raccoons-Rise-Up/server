using System;

namespace GameServer.Console
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
            var prevCursorLeft = System.Console.CursorLeft;
            Clear(false);

            System.Console.WriteLine(input);
            System.Console.CursorLeft = prevCursorLeft;
        }

        /// <summary>
        /// Move the text field down
        /// </summary>
        public void MoveDown()
        {
            var prevCursorLeft = System.Console.CursorLeft;
            Clear(false);
            System.Console.CursorTop++;

            System.Console.WriteLine(input);

            System.Console.CursorLeft = prevCursorLeft;
        }

        /// <summary>
        /// Clear the text field display and optionally clear the input
        /// </summary>
        public void Clear(bool clearInput)
        {
            if (clearInput)
                input = "";

            System.Console.CursorLeft = 0;
            System.Console.Write(new string(' ', System.Console.WindowWidth));
            System.Console.CursorTop--;
        }
    }
}
