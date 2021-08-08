using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using GameServer.Logging.Commands;
using System.Runtime.InteropServices;

namespace GameServer.Logging
{
    public class Logger
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

        // Command History
        private static readonly List<string> s_CommandHistory = new();
        private static int s_CommandHistoryIndex;
        private const byte c_MaxHistoryCommands = 255;

        // Text Field
        private static readonly LoggerTextField s_TextField = new();
        private static readonly object s_ThreadLock = new();
        private static int s_SpaceBarCount;

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

            var message = new LoggerMessage(str);

            s_Messages.Enqueue(message);
        }

        public static void MessagesThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            while (true)
            {
                Thread.Sleep(c_MessageThreadTickRate); // Otherwise CPU resources will go to waste

                lock (s_ThreadLock) 
                {
                    while (s_Messages.TryDequeue(out LoggerMessage message))
                    {
                        s_TextField.MoveDown();
                        AddMessageToConsole(message);
                    }
                }
            }
        }

        public static void InputThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            Terminal.DisableMinimize();
            Terminal.DisableMaximize();
            Terminal.DisableResize();

            s_TextField.m_Row = 1; // Keep the text field 1 row ahead of the logged messages

            while (true)
            {
                lock (s_ThreadLock)
                    Console.CursorTop = s_TextField.m_Row;

                var keyInfo = Console.ReadKey(true);

                lock (s_ThreadLock) 
                {
                    if (keyInfo.Key == ConsoleKey.Spacebar) 
                        s_SpaceBarCount++;

                    if (keyInfo.Key == ConsoleKey.Delete) 
                    {
                        var input = s_TextField.m_Input;

                        if (input == "" || Console.CursorLeft == input.Length)
                            continue;

                        Console.Write(' ');
                        Console.CursorLeft--;

                        // Update the input variable
                        
                        var cursorColumn = input.Length - 1 - Console.CursorLeft;
                        s_TextField.m_Input = input.Remove(input.Length - 1 - cursorColumn, 1);

                        // Since the input was edited, it needs to be redrawn
                        s_TextField.Redraw();

                        continue;
                    }

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
                        var input = s_TextField.m_Input;
                        var cursorColumn = input.Length - 1 - Console.CursorLeft;
                        s_TextField.m_Input = input.Remove(input.Length - 1 - cursorColumn, 1);

                        // Since the input was edited, it needs to be redrawn
                        s_TextField.Redraw();
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.LeftArrow) 
                    {
                        // Stay within the console window bounds
                        if (Console.CursorLeft <= 0)
                            continue;

                        Console.CursorLeft--;
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        // Stay within the console window bounds
                        if (Console.CursorLeft >= Console.WindowWidth - 1)
                            continue;

                        Console.CursorLeft++;
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

                        s_TextField.m_Input = nextCommand;
                        s_TextField.Clear(false);

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

                        s_TextField.m_Input = prevCommand;
                        s_TextField.Clear(false);

                        Console.WriteLine(prevCommand);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Enter) 
                    {
                        var inputArr = s_TextField.m_Input.Trim().ToLower().Split(' ');
                        var cmd = inputArr[0];
                        var args = inputArr.Skip(1).ToArray();

                        // If the user spams spacebar but the cmd is still empty, reset the user input if enter is pressed
                        if (cmd == "" && s_SpaceBarCount > 0) 
                        {
                            s_SpaceBarCount = 0;
                            Console.CursorLeft = 0;
                            s_TextField.m_Column = 0;
                            s_TextField.m_Input = "";
                            continue;
                        }

                        // Do not do anything if command string is empty
                        if (cmd == "")
                            continue;

                        if (s_Commands.ContainsKey(cmd))
                            s_Commands[cmd].Run(args);
                        else
                            Log($"Unknown Command: '{cmd}'");

                        // Only keep track of a set of previously entered commands
                        if (s_CommandHistory.Count > c_MaxHistoryCommands) 
                            s_CommandHistory.RemoveAt(0);

                        // Add command to command history
                        s_CommandHistory.Add(cmd);

                        // Reset command history index to 0 on enter
                        s_CommandHistoryIndex = 0;

                        // Reset input and text field input
                        s_TextField.Clear(true);
                        s_SpaceBarCount = 0;
                        continue;
                    }

                    // Write the character to the console and input, also keep track of text field column
                    Console.Write(keyInfo.KeyChar);

                    s_TextField.m_Input += keyInfo.KeyChar;
                    s_TextField.m_Column++;

                    if (Console.CursorLeft >= Console.WindowWidth - 1) 
                    {
                        // TODO: Make input text field scroll horizontally
                    }
                }
            }
        }

        private static void AddMessageToConsole(LoggerMessage message) 
        {
            // Set the cursor to the logger area
            var prevTextFieldColumn = Console.CursorLeft;

            // Add the message to the logger area
            WriteColoredMessage(message.m_Text);

            // Reset cursor position to the text field area
            Console.CursorTop++; // Move down more to get back to text field input

            // Calculate the number of lines needed from this message
            var lines = message.GetLines();

            // Move the logger area and text field area down by 'lines'
            s_LoggerMessageRow += lines;
            s_TextField.m_Row += lines;

            // Reset text field column
            s_TextField.m_Column = 0;

            // Put cursor left back to where it previously was
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

            Console.Write(Environment.NewLine);
        }

        private static void ResetColor() 
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        // TEST MESSAGE (2 lines)
        // TheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefenceTheredfoxjumpedoverthefence
    }
}
