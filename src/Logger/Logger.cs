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
        public static readonly ConcurrentQueue<string> s_Messages = new();

        private static readonly object s_ThreadLock = new();
        private static string s_Input = "";
        private static int s_LoggerMessageRow = 0;
        private static int s_TextFieldColumn = 0;
        private static int s_TextFieldRow = 1;
        private static int s_TextFieldHeight = 0; // For multi-line input text field

        public static void MessagesThread()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(100).Wait(); // Otherwise CPU resources will go to waste

                    while (s_Messages.TryDequeue(out string message))
                    {


                    }

                    Thread.Sleep(3000);
                    lock (s_ThreadLock) 
                    {
                        MoveTextInputFieldDown();
                        AddMessageToConsole("Test", false);
                    }
                }
            });
        }

        public static void InputThread()
        {
            while (true)
            {
                lock (s_ThreadLock)
                    Console.CursorTop = s_TextFieldRow + s_TextFieldHeight;

                var keyInfo = Console.ReadKey(true);

                lock (s_ThreadLock) 
                {
                    switch (keyInfo.Key) 
                    {
                        case ConsoleKey.OemPlus:
                            MoveTextInputFieldDown();
                            AddMessageToConsole("Hello world", false);
                            continue;
                        case ConsoleKey.Enter:
                            ClearTextInputField();
                            AddMessageToConsole(s_Input, true);
                            s_Input = "";
                            continue;
                    }

                    Console.Write(keyInfo.KeyChar);
                    s_TextFieldColumn++;

                    // Go to next line if text is long as window width
                    if (Console.CursorLeft == Console.WindowWidth - 1)
                    {
                        s_TextFieldHeight++;
                        Console.CursorLeft = 0;
                        s_TextFieldColumn = 0;
                    }

                    s_Input += keyInfo.KeyChar;
                }
            }
        }

        private static void AddMessageToConsole(string message, bool fromUserKeyboard) 
        {
            // Set the cursor to the logger area
            Console.CursorLeft = 0;
            Console.CursorTop = s_LoggerMessageRow;
            Console.WriteLine(message); // Console.WriteLine will place a new line character (Console.WriteLine also ensures no funny business happens when resizing the terminal)
            Console.CursorTop += (1 + s_TextFieldHeight); // Move down more to get back to text field input

            var lines = (int)Math.Ceiling((float)message.Length / Console.WindowWidth);

            s_LoggerMessageRow += lines;
            s_TextFieldRow += lines;
            
            if (fromUserKeyboard)
            {
                s_TextFieldColumn = 0;
                s_TextFieldHeight = 0;
            }
            else 
            {
                Console.CursorLeft = s_TextFieldColumn;
            }
        }

        private static void MoveTextInputFieldDown() 
        {
            // Move the text input field
            Console.CursorTop -= s_TextFieldHeight;

            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));

            //textFieldRow += 1;

            Console.Write(s_Input);
        }

        private static void ClearTextInputField() 
        {
            // Clear the text input field
            for (int i = 0; i < s_TextFieldHeight + 1; i++)
            {
                Console.CursorLeft = 0;
                Console.Write(new string(' ', Console.WindowWidth));
                Console.CursorTop -= 2;
                
            }
        }

        // The red fox jumped over the fence and he was happy because of his nice life in the world of blues of life of world of happiness and prosperity yes indeed!
    }
}
