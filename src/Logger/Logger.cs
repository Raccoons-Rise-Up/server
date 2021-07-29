using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Logger
    {
        public static readonly ConcurrentQueue<string> m_Messages = new();

        private static readonly object threadLock = new();
        private static string input = "";
        private static int loggerMessageRow = 0;
        private static int textFieldRow = 1;
        private static int textFieldRowExpansion = 0;

        public static void MessagesThread()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(100).Wait(); // Otherwise CPU resources will go to waste

                    while (m_Messages.TryDequeue(out string message))
                    {


                    }

                    Thread.Sleep(3000);
                    lock (threadLock) 
                    {
                        MoveTextInputFieldDown(input);
                        Log("Test");
                    }
                }
            });
        }

        public static void InputThread()
        {
            while (true)
            {
                lock (threadLock)
                    Console.CursorTop = textFieldRow + textFieldRowExpansion;

                var keyInfo = Console.ReadKey(true);

                lock (threadLock) 
                {
                    Console.Write(keyInfo.KeyChar);

                    // Go to next line if text is long as window width
                    if (Console.CursorLeft == Console.WindowWidth - 1)
                    {
                        textFieldRowExpansion++;
                        Console.CursorLeft = 0;
                    }

                    switch (keyInfo.Key) 
                    {
                        case ConsoleKey.Enter:
                            ClearTextInputField();
                            Log(input);
                            input = "";
                            break;
                        case ConsoleKey.OemMinus:
                            // TEST
                            //MoveTextInputFieldDown(input);
                            break;
                        case ConsoleKey.OemPlus:
                            // TEST
                            MoveTextInputFieldDown(input);
                            Log("Hello world");
                            break;
                        default:
                            var ch = keyInfo.KeyChar;
                            input += ch;
                            break;
                    }
                }
            }
        }

        private static void Log(string message) 
        {
            // Set the cursor to the logger area
            Console.CursorLeft = 0;
            Console.CursorTop = loggerMessageRow;
            Console.WriteLine(message); // Console.WriteLine will place a new line character (Console.WriteLine also ensures no funny business happens when resizing the terminal)
            Console.CursorTop += 1; // Move down one more to get back to text field input
            // TODO: Keep track of CursorLeft for text field input

            var lines = (int)Math.Ceiling((float)message.Length / Console.WindowWidth);

            loggerMessageRow += lines;
            textFieldRow += lines;
            textFieldRowExpansion = 0;
        }

        private static void MoveTextInputFieldDown(string input) 
        {
            // Move the text input field
            Console.CursorTop -= textFieldRowExpansion;

            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));

            //textFieldRow += 1;

            Console.Write(input);
        }

        private static void ClearTextInputField() 
        {
            // Clear the text input field
            for (int i = 0; i < textFieldRowExpansion + 1; i++)
            {
                Console.Write(new string(' ', Console.WindowWidth));
                Console.CursorTop -= 2;
                Console.CursorLeft = 0;
            }
        }

        // The red fox jumped over the fence and he was happy because of his nice life in the world of blues of life of world of happiness and prosperity yes indeed!
    }
}
