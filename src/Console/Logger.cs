using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using GameServer.Logging.Commands;
using GameServer.Util;

namespace GameServer.Logging
{
    public static class Logger
    {
        // Commands
        private static readonly Dictionary<string, Command> s_Commands = typeof(Command).Assembly.GetTypes()
            .Where(x => typeof(Command)
            .IsAssignableFrom(x) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<Command>()
            .ToDictionary(x => x.GetType().Name
            .Replace("Command", "")
            .ToLower(), x => x);

        // History
        private const byte c_MaxHistoryCommands = 255;
        private static int s_CommandIndex;
        private static readonly CircularBuffer<string> s_CommandHistory = new(c_MaxHistoryCommands);


        // Text Field
        private static string s_Input = "";

        private static readonly object s_ThreadLock = new();

        // Logging
        private static readonly ConcurrentQueue<LoggerMessage> s_Messages = new();
        private static int s_LoggerMessageRow;
        private const int c_MessageThreadTickRate = 200;

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
            var str = $"{color}{time} [{thread}] [{logLevel}] {obj}";

            s_Messages.Enqueue(new LoggerMessage(str));
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
                    Log("&yTe&rst");
                }
            }
        }

        public static void MessagesThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            new Thread(TestThread).Start(); // temporary

            while (true)
            {
                Thread.Sleep(c_MessageThreadTickRate);

                lock (s_ThreadLock) 
                {
                    while (s_Messages.TryDequeue(out LoggerMessage message))
                        AddMessageToConsole(message);
                }
            }
        }

        public static void InputThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            Console.Clear();
            Console.WriteLine();

            while (true)
            {
                var keyInfo = Console.ReadKey(true);

                lock (s_ThreadLock) 
                {
                    if (keyInfo.Key == ConsoleKey.Delete) 
                    {
                        if (s_Input == "")
                            continue;

                        Console.Write(' ');
                        Console.CursorLeft--;
                        
                        var cursorColumn = s_Input.Length - 1 - Console.CursorLeft;
                        s_Input = s_Input.Remove(s_Input.Length - 1 - cursorColumn, 1);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Backspace) 
                    {
                        if (Console.CursorLeft <= 0)
                            continue;
                        
                        Console.CursorLeft--;
                        Console.Write(' ');
                        Console.CursorLeft--;

                        var cursorColumn = s_Input.Length - 1 - Console.CursorLeft;
                        s_Input = s_Input.Remove(s_Input.Length - 1 - cursorColumn, 1);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.LeftArrow) 
                    {
                        if (Console.CursorLeft <= 0)
                            continue;

                        Console.CursorLeft--;
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        if (Console.CursorLeft >= Console.WindowWidth - 1)
                            continue;

                        Console.CursorLeft++;
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (s_CommandHistory.Size < 1)
                            continue;

                        if (s_CommandIndex <= 1)
                            continue;

                        s_Input = s_CommandHistory[s_CommandHistory.Size - --s_CommandIndex];

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (s_CommandHistory.Size < 1)
                            continue;

                        if (s_CommandIndex >= s_CommandHistory.Size)
                            continue;

                        var prevCommand = s_CommandHistory[s_CommandHistory.Size - 1 - s_CommandIndex++];

                        s_Input = prevCommand;

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Enter) 
                    {
                        var inputArr = s_Input.Trim().ToLower().Split(' ');
                        var cmd = inputArr[0];
                        var args = inputArr.Skip(1).ToArray();

                        s_Input = "";
                        DrawInput(0);

                        if (cmd == "")
                            continue;

                        if (s_Commands.ContainsKey(cmd))
                            s_Commands[cmd].Run(args);
                        else
                            Log($"Unknown Command: '{cmd}'");

                        // Add command to command history
                        s_CommandHistory.Push(cmd);

                        // Reset command history index to 0 on enter
                        s_CommandIndex = 0;

                        // Reset input and text field input
                        continue;
                    }

                    s_Input = s_Input.Insert(Console.CursorLeft, keyInfo.KeyChar.ToString()).ToString();
                    DrawInput(Console.CursorLeft + 1);
                    

                    if (Console.CursorLeft >= Console.WindowWidth - 1) 
                    {
                        // TODO: Make input text field scroll horizontally
                    }
                }
            }
        }

        private static void DrawInput(int col) 
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.CursorLeft = 0;
            Console.Write(s_Input);

            if (col < Console.BufferWidth)
                Console.CursorLeft = col;
        }

        private static void AddMessageToConsole(LoggerMessage message) 
        {
            var prevTextFieldColumn = Console.CursorLeft;

            Console.CursorTop--;
            Console.CursorLeft = 0;

            Console.WriteLine(message.m_Text);
            Console.WriteLine(new string(' ', Console.WindowWidth)); 

            Console.Write(s_Input);

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
                        Console.ForegroundColor = LoggerColor.s_NumColorCodes[intColorCode];
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

                    foreach (var entry in LoggerColor.s_CharColorCodes)
                    {
                        if (charColorCode == entry.Key)
                        {
                            Console.ForegroundColor = LoggerColor.s_CharColorCodes[charColorCode];
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

            Console.WriteLine();
        }

        private static void ResetColor() 
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        // TEST MESSAGE (2 lines)
        // The red fox jumped over the fence and he was happy because of his nice life in the world of blues of life of world of happiness and prosperity yes indeed!
    }
}
