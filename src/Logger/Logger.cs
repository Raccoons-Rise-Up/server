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
            var textFieldRow = 1;
            var textFieldRowExpansion = 0;

            var input = "";

            //Program.StartServer(); // Finished setting up Logger, now start the server

            //m_Messages.Enqueue("Hello world");

            while (true)
            {
                /*while (m_Messages.TryDequeue(out string message))
                {
                    //Console.WriteLine(message);
                }*/

                Console.CursorTop = textFieldRow + textFieldRowExpansion;
                var keyInfo = Console.ReadKey(true);
                Console.Write(keyInfo.KeyChar);
                if (Console.CursorLeft == Console.WindowWidth - 1) 
                {
                    textFieldRowExpansion++;
                    Console.CursorLeft = 0;
                }
                    

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    // Clear the text input field
                    for (int i = 0; i < textFieldRowExpansion + 1; i++) 
                    {
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.CursorTop -= 2;
                        Console.CursorLeft = 0;
                    }


                    Console.CursorTop = loggerMessageRow;
                    Console.WriteLine(input);

                    var lines = (int)Math.Ceiling((float)input.Length / Console.WindowWidth);
                    input = "";

                    

                    loggerMessageRow += lines;
                    textFieldRow += lines;
                    textFieldRowExpansion = 0;
                }
                else 
                {
                    var ch = keyInfo.KeyChar;
                    input += ch;
                }
            }
        }
    }
}
