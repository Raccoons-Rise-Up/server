using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class Logger
    {
        public static ConcurrentQueue<string> m_Messages = new ConcurrentQueue<string>();

        public static void WorkerThread()
        {
            var loggerMessageRow = 0;
            //var textFieldRow = Console.WindowHeight - 2;
            var textFieldRow = 10;

            var messageLines = 0;

            //Program.StartServer(); // Finished setting up Logger, now start the server

            m_Messages.Enqueue("Hello world");
            //new Thread(Test).Start();

            while (true)
            {
                while (m_Messages.TryDequeue(out string message))
                {
                    //Console.WriteLine(message);
                }

                

                // Set cursor to the beginning of the text input field
                Console.SetCursorPosition(0, textFieldRow);

                // Clear the text input field
                Console.Write(new string(' ', Console.WindowWidth));

                // If the logger message row is >= text field row then move the text field row one down
                if (loggerMessageRow >= textFieldRow)
                    textFieldRow += messageLines;

                // Set cursor back to the beginning of the text input field
                Console.SetCursorPosition(0, textFieldRow);

                // Read the command the user typed and pressed enter
                var input = Console.ReadLine();

                var lines = (int)Math.Ceiling(input.Length / (float)Console.WindowWidth);

                // Set cursor to logging area
                Console.SetCursorPosition(0, loggerMessageRow);

                // Print the message to the logger (Console.Write() is not used as everything would get distorted the window gets resized)
                Console.WriteLine(input);

                // Increment row by the number of lines from input
                loggerMessageRow += lines;
                messageLines = lines;
            }
        }

        private static void Test() 
        {
            while (true) 
            {
                Thread.Sleep(1000);
                Console.WriteLine("Yes");
            }
        }
    }
}
