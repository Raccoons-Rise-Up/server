using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace GameServer
{
    public class Logger
    {
        // Colors
        private static readonly ConsoleColor[] s_NumColors = new ConsoleColor[10] // there can only be up to 10 num color codes [0..9]
        {
            ConsoleColor.Black,        // 0
            ConsoleColor.DarkGray,     // 1 (ConsoleColor.DarkGray and ConsoleColor.Gray seem to be the same color)
            ConsoleColor.Gray,         // 2 ('g' conflicts with Green color)
            ConsoleColor.DarkMagenta,  // 3
            ConsoleColor.DarkBlue,     // 4
            ConsoleColor.DarkCyan,     // 5
            ConsoleColor.DarkGreen,    // 6
            ConsoleColor.DarkYellow,   // 7
            ConsoleColor.DarkRed,      // 8
            ConsoleColor.Red           // 9 ('r' is reserved for resetting the colors, 'r' also conflicts with Red color)
        };
        private static readonly Dictionary<char, ConsoleColor> s_CharColors = new()
        {
            { 'b', ConsoleColor.Blue },
            { 'c', ConsoleColor.Cyan },
            { 'g', ConsoleColor.Green },
            { 'm', ConsoleColor.Magenta },
            { 'y', ConsoleColor.Yellow }
        };

        // Command History
        private static readonly List<string> s_CommandHistory = new();
        private static int s_CommandHistoryIndex = 0;
        private const byte s_MaxHistoryCommands = 255;

        // Text Field
        private static TextField s_TextField = new();
        private static readonly object s_ThreadLock = new();

        // Logging
        private static readonly ConcurrentQueue<string> s_Messages = new();
        private static int s_LoggerMessageRow = 0;

        public static void LogError(object obj) 
        {
            Log(obj, "ERROR", "&8");
        }

        public static void LogWarning(object obj)
        {
            Log(obj, "WARN", "&9");
        }

        public static void Log(object obj, string logLevel = "INFO", string color = "&r")
        {
            var time = $"{DateTime.Now:HH:mm:ss}";
            var thread = Thread.CurrentThread.Name;
            var message = $"{color}{time} [{thread}] [{logLevel}] {obj}";

            s_Messages.Enqueue(message);
        }

        private static void TestThread() 
        {
            Thread.CurrentThread.Name = "TEST";

            while (true) 
            {
                // Test message is sent every 3 seconds for debugging
                Thread.Sleep(3000);
                lock (s_ThreadLock)
                {
                    //Log("&yTe&rst");
                }
            }
        }

        public static void MessagesThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            new Thread(TestThread).Start(); // temporary

            while (true)
            {
                Thread.Sleep(200); // Otherwise CPU resources will go to waste

                lock (s_ThreadLock) 
                {
                    while (s_Messages.TryDequeue(out string message))
                    {
                        MoveTextInputFieldDown();
                        AddMessageToConsole(message);
                    }
                }
            }
        }

        public static void InputThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            s_TextField.m_Row = 1; // Keep the text field 1 row ahead of the logged messages

            while (true)
            {
                lock (s_ThreadLock)
                    Console.CursorTop = s_TextField.m_Row + s_TextField.m_Height;

                var keyInfo = Console.ReadKey(true);

                lock (s_ThreadLock) 
                {
                    if (keyInfo.Key == ConsoleKey.Backspace) 
                    {
                        // Stay within the console window bounds
                        if (Console.CursorLeft <= 0)
                            continue;
                        
                        // Delete the character and move back one space
                        Console.CursorLeft--;
                        Console.Write(' ');
                        Console.CursorLeft--;

                        // Update the input variable
                        s_TextField.m_Input = s_TextField.m_Input.Remove(s_TextField.m_Input.Length - 1, 1);
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.LeftArrow) 
                    {
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (s_CommandHistory.Count < 1)
                            continue;

                        if (s_CommandHistoryIndex <= 1)
                            continue;

                        s_CommandHistoryIndex--;
                        var nextCommand = s_CommandHistory[s_CommandHistory.Count - s_CommandHistoryIndex];

                        ClearTextInputField();
                        Console.CursorTop++;

                        s_TextField.m_Input = nextCommand;
                        Console.WriteLine(nextCommand);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (s_CommandHistory.Count < 1)
                            continue;

                        if (s_CommandHistoryIndex >= s_CommandHistory.Count)
                            continue;

                        var prevCommand = s_CommandHistory[s_CommandHistory.Count - 1 - s_CommandHistoryIndex];
                        s_CommandHistoryIndex++;

                        ClearTextInputField();
                        Console.CursorTop++;

                        s_TextField.m_Input = prevCommand;
                        Console.WriteLine(prevCommand);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Enter) 
                    {
                        ClearTextInputField();
                        // TODO: Handle commands here...

                        if (s_CommandHistory.Count > s_MaxHistoryCommands) 
                            s_CommandHistory.RemoveAt(0);

                        s_CommandHistoryIndex = 0;
                        s_CommandHistory.Add(s_TextField.m_Input);
                        s_TextField.m_Input = "";
                        continue;
                    }

                    Console.Write(keyInfo.KeyChar);
                    s_TextField.m_Column++;

                    // Go to next line if text is long as window width
                    if (Console.CursorLeft == Console.WindowWidth - 1)
                    {
                        s_TextField.m_Height++;
                        Console.CursorLeft = 0;
                        s_TextField.m_Column = 0;
                    }

                    s_TextField.m_Input += keyInfo.KeyChar;
                }
            }
        }

        private static void AddMessageToConsole(string message) 
        {
            // Set the cursor to the logger area
            var prevTextFieldColumn = Console.CursorLeft;

            // Add the message to the logger area
            WriteColoredMessage(message);

            // Reset cursor position to the text field area
            Console.CursorTop += (1 + s_TextField.m_Height); // Move down more to get back to text field input

            var lines = (int)Math.Ceiling((float)message.Length / Console.WindowWidth);

            s_LoggerMessageRow += lines;
            s_TextField.m_Row += lines;

            s_TextField.m_Column = 0;
            //s_TextField.m_Height = 0;
            //Console.CursorLeft = s_TextField.m_Column - s_TextField.m_Height; // Is this line really necessary?

            Console.CursorLeft = prevTextFieldColumn;
        }

        private static void WriteColoredMessage(string message) 
        {
            Console.CursorLeft = 0;
            Console.CursorTop = s_LoggerMessageRow;

            // Check for color codes in the message
            if (message.Contains('&'))
            {
                // The beginning of the message
                var noColorEnd = message.IndexOf('&');
                var beginningNoColorMessage = message.Substring(0, noColorEnd);

                // The rest of the message
                var remainingMessage = message[noColorEnd..];
                var remainingWords = remainingMessage.Split('&');

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(beginningNoColorMessage);

                foreach (var word in remainingWords)
                {
                    if (word == "")
                        continue;

                    // Check for valid int color code after &
                    if (int.TryParse(word[0..1], out int intColorCode))
                    {
                        Console.ForegroundColor = s_NumColors[intColorCode];
                        Console.Write(word[1..]);
                        ResetColor();
                        continue;
                    }

                    // Check for valid char color code after &
                    var charColorCode = char.Parse(word[0..1]);

                    if (charColorCode == 'r')
                    {
                        ResetColor();
                        Console.Write(word[1..]);
                        continue;
                    }

                    var foundCharColorCode = false;

                    foreach (var entry in s_CharColors)
                    {
                        if (charColorCode == entry.Key)
                        {
                            Console.ForegroundColor = s_CharColors[charColorCode];
                            Console.Write(word[1..]);
                            ResetColor();
                            foundCharColorCode = true;
                            break;
                        }
                    }

                    // Did not find any color codes
                    if (!foundCharColorCode)
                    {
                        Console.Write(word);
                        ResetColor(); // Just for added measure
                    }
                }
            }
            else
            {
                // There are no color codes in the message, simply write it to the console
                Console.Write(message);
            }

            Console.Write(Environment.NewLine);
        }

        private static void ResetColor() 
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void MoveTextInputFieldDown() 
        {
            ClearTextInputField();
            Console.CursorTop += 2;

            Console.Write(s_TextField.m_Input);
        }

        private static void ClearTextInputField() 
        {
            // Clear the text input field
            for (int i = 0; i < s_TextField.m_Height + 1; i++)
            {
                Console.CursorLeft = 0;
                Console.Write(new string(' ', Console.WindowWidth));
                Console.CursorTop -= 2;
            }
        }

        private struct TextField
        {
            public string m_Input;
            public int m_Column;
            public int m_Row;
            public int m_Height;
        }

        // TEST MESSAGE (2 lines)
        // The red fox jumped over the fence and he was happy because of his nice life in the world of blues of life of world of happiness and prosperity yes indeed!
    }
}
