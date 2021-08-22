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
        private static readonly Dictionary<string, Command> commands = typeof(Command).Assembly.GetTypes()
            .Where(x => typeof(Command)
            .IsAssignableFrom(x) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<Command>()
            .ToDictionary(x => x.GetType().Name
            .Replace("Command", "")
            .ToLower(), x => x);

        // Command History
        private static readonly List<string> commandHistory = new();
        private static int commandHistoryIndex;
        private const byte maxHistoryCommands = 255;

        // Text Field
        private static readonly LoggerTextField textField = new();
        private static readonly object threadLock = new();
        private static int spaceBarCount;

        // Logging
        private static readonly ConcurrentQueue<LoggerMessage> messages = new();
        private static int loggerMessageRow;
        private const int messageThreadTickRate = 200;

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

            messages.Enqueue(message);
        }

        public static void MessagesThread()
        {
            Thread.CurrentThread.Name = "CONSOLE";

            while (true)
            {
                Thread.Sleep(messageThreadTickRate); // Otherwise CPU resources will go to waste

                lock (threadLock) 
                {
                    while (messages.TryDequeue(out LoggerMessage message))
                    {
                        textField.MoveDown();
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

            textField.row = 1; // Keep the text field 1 row ahead of the logged messages

            while (true)
            {
                lock (threadLock)
                    Console.CursorTop = textField.row;

                var keyInfo = Console.ReadKey(true);

                lock (threadLock) 
                {
                    if (keyInfo.Key == ConsoleKey.Spacebar) 
                        spaceBarCount++;

                    if (keyInfo.Key == ConsoleKey.Delete) 
                    {
                        var input = textField.input;

                        if (input == "" || Console.CursorLeft == input.Length)
                            continue;

                        Console.Write(' ');
                        Console.CursorLeft--;

                        // Update the input variable
                        
                        var cursorColumn = input.Length - 1 - Console.CursorLeft;
                        textField.input = input.Remove(input.Length - 1 - cursorColumn, 1);

                        // Since the input was edited, it needs to be redrawn
                        textField.Redraw();

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
                        var input = textField.input;
                        var cursorColumn = input.Length - 1 - Console.CursorLeft;
                        textField.input = input.Remove(input.Length - 1 - cursorColumn, 1);

                        // Since the input was edited, it needs to be redrawn
                        textField.Redraw();
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
                        if (commandHistory.Count < 1)
                            continue;

                        if (commandHistoryIndex <= 1)
                            continue;

                        commandHistoryIndex--;
                        var nextCommand = commandHistory[commandHistory.Count - commandHistoryIndex];

                        textField.input = nextCommand;
                        textField.Clear(false);

                        Console.WriteLine(nextCommand);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (commandHistory.Count < 1)
                            continue;

                        if (commandHistoryIndex >= commandHistory.Count)
                            continue;

                        var prevCommand = commandHistory[commandHistory.Count - 1 - commandHistoryIndex];
                        commandHistoryIndex++;

                        textField.input = prevCommand;
                        textField.Clear(false);

                        Console.WriteLine(prevCommand);

                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Enter) 
                    {
                        if (textField.input == null)
                            continue;

                        var inputArr = textField.input.Trim().ToLower().Split(' ');
                        var cmd = inputArr[0];
                        var args = inputArr.Skip(1).ToArray();

                        // If the user spams spacebar but the cmd is still empty, reset the user input if enter is pressed
                        if (cmd == "" && spaceBarCount > 0) 
                        {
                            spaceBarCount = 0;
                            Console.CursorLeft = 0;
                            textField.column = 0;
                            textField.input = "";
                            continue;
                        }

                        // Do not do anything if command string is empty
                        if (cmd == "")
                            continue;

                        if (commands.ContainsKey(cmd))
                            commands[cmd].Run(args);
                        else
                            Log($"Unknown Command: '{cmd}'");

                        // Only keep track of a set of previously entered commands
                        if (commandHistory.Count > maxHistoryCommands) 
                            commandHistory.RemoveAt(0);

                        // Add command along with args[] to command history
                        commandHistory.Add(textField.input);

                        // Reset command history index to 0 on enter
                        commandHistoryIndex = 0;

                        // Reset input and text field input
                        textField.Clear(true);
                        spaceBarCount = 0;
                        continue;
                    }

                    // Write the character to the console and input, also keep track of text field column
                    Console.Write(keyInfo.KeyChar);

                    textField.input += keyInfo.KeyChar;
                    textField.column++;

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
            WriteColoredMessage(message.text);

            // Reset cursor position to the text field area
            Console.CursorTop++; // Move down more to get back to text field input

            // Calculate the number of lines needed from this message
            var lines = message.GetLines();

            // Move the logger area and text field area down by 'lines'
            loggerMessageRow += lines;
            textField.row += lines;

            // Reset text field column
            textField.column = 0;

            // Put cursor left back to where it previously was
            Console.CursorLeft = prevTextFieldColumn;
        }

        private static void WriteColoredMessage(string message) 
        {
            Console.CursorLeft = 0;
            Console.CursorTop = loggerMessageRow;

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
                        Console.ForegroundColor = LoggerColor.numColorCodes[intColorCode];
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

                    foreach (var entry in LoggerColor.charColorCodes)
                    {
                        if (charColorCode == entry.Key)
                        {
                            Console.ForegroundColor = LoggerColor.charColorCodes[charColorCode];
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
