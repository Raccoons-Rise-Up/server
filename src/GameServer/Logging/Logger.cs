using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

using Terminal.Gui;

using GameServer.Logging.Commands;
using GameServer.Networking;

namespace GameServer.Logging
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public class Logger
    {
        public static Dictionary<string, Command> Commands = typeof(Command).Assembly.GetTypes().Where(x => typeof(Command).IsAssignableFrom(x) && !x.IsAbstract).Select(Activator.CreateInstance).Cast<Command>().ToDictionary(x => x.GetType().Name.ToLower(), x => x);
        private static readonly ConcurrentDictionary<LogType, (Terminal.Gui.Attribute Color, string Name)> typeColor = new ConcurrentDictionary<LogType, (Terminal.Gui.Attribute Color, string Name)>();

        public static TextField Input;
        public static LoggerView View;
        public static ConcurrentQueue<LoggerMessage> Messages;
        public static int ViewOffset = 0;

        public static List<string> CommandHistory;
        public static int CommandHistoryIndex = 0;

        public void Start()
        {   
            Application.Init();
            Application.OnResized += UpdatePositions;

            // Populate concurrent type color dictionary
            typeColor[LogType.Info] = (Application.Driver.MakeAttribute(Color.White, Color.Black), "INFO");
            typeColor[LogType.Warning] = (Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black), "WARNING");
            typeColor[LogType.Error] = (Application.Driver.MakeAttribute(Color.Red, Color.Black), "ERROR");

            CommandHistory = new List<string>();
            Messages = new ConcurrentQueue<LoggerMessage>();
            View = new LoggerView();

            Application.GrabMouse(View); // This way we can scroll even when our mouse is over Label views
            Application.Top.Add(View);

            CreateInputField();

            StartServer(); // Finished setting up Logger, we can now start the server

            Application.Run();
        }

        public void StartServer()
        {
            Program.Server = new Server(7777, 100);
            new Thread(Program.Server.Start).Start(); // Initialize server on thread 2
        }

        private void CreateInputField()
        {
            Input = new TextField("")
            {
                X = 0,
                Y = LoggerView.Driver.Clip.Bottom - 1,
                Width = LoggerView.Driver.Clip.Width
            };

            View.Add(Input);
        }

        public static void LogError(object obj) 
        {
            AddMessage(CreateMessage(LogType.Error, obj));
        }

        public static void LogWarning(object obj) 
        {
            AddMessage(CreateMessage(LogType.Warning, obj));
        }

        public static void Log(object obj)
        {
            AddMessage(CreateMessage(LogType.Info, obj));
        }

        private static LoggerMessage CreateMessage(LogType type, object obj) 
        {
            if (obj == null) obj = "undefined";
            var time = $"{DateTime.Now:HH:mm:ss}";
            var message = new LoggerMessage($"{time} [{typeColor[LogType.Info].Name}] {obj.ToString()}");
            message.TextColor = typeColor[LogType.Info].Color;
            return message;
        }

        private static void AddMessage(LoggerMessage message) 
        {
            const int BOTTOM_PADDING = 3;
            if (GetTotalLines() > LoggerView.Driver.Clip.Bottom - BOTTOM_PADDING)
                ViewOffset -= message.GetLines();

            Messages.Enqueue(message);
            View.Add(message);
            UpdatePositions();
        }

        public static void UpdatePositions()
        {
            // Update message positions
            var index = 0;

            foreach (var message in Messages)
            {
                message.Y = index + ViewOffset;
                index += message.GetLines();
            }

            // Update input text field position
            Input.Y = index + ViewOffset;

            Application.Refresh();
        }

        public static int GetTotalLines()
        {
            var totalLines = 0;
            foreach (var message in Messages)
            {
                totalLines += message.GetLines();
            }
            return totalLines;
        }

        public static void HandleCommands(string input)
        {
            var cmd = input.ToLower().Split(' ')[0];
            var args = input.ToLower().Split(' ').Skip(1).ToArray();

            if (Commands.ContainsKey(cmd))
                Commands[cmd].Run(args);
            else
                Log($"Unknown Command: '{cmd}'");
        }
    }
}