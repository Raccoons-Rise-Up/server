using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

using Attribute = Terminal.Gui.Attribute; // Fixes error CS0104	'Attribute' is an ambiguous reference between 'Terminal.Gui.Attribute' and 'System.Attribute'

namespace GameServer
{
    public static class Logger
    {
        public static ConcurrentQueue<string> m_Messages = new ConcurrentQueue<string>();

        public static void WorkerThread()
        {
            Application.Init();

            // The main console view
            var view = new LoggerView
            {
                X = 0,
                Y = 0,
                Width = Application.Driver.Clip.Width,
                Height = Application.Driver.Clip.Height
            };
            Application.GrabMouse(view);
            Application.Top.Add(view);

            CreateInputField(view);

            

            var label = new Label("Hello World")
            {
                X = 0,
                Y = 0,
                Width = view.Width,
                Height = 1
            };

            var label2 = new Label("bye world")
            {
                X = 0,
                Y = 1,
                Width = view.Width,
                Height = 1
            };

            view.Add(label);
            view.Add(label2);

            Program.StartServer(); // Finished setting up Logger, now start the server

            while (true)
            {
                while (m_Messages.TryDequeue(out string message))
                {
                    AddLabel(view, message);
                }
            }

            Application.Run();

        }

        private static void AddLabel(LoggerView view, object obj) 
        {
            var label = new Label(obj.ToString())
            {
                X = 0,
                Y = 2,
                Width = view.Width,
                Height = 1
            };

            view.Add(label);
        }

        private static void Log(object obj) 
        {
            var time = $"{DateTime.Now:HH:mm:ss}";
            var message = $"{time} {obj}";

            m_Messages.Enqueue(message);
        }

        private static void CreateInputField(LoggerView view)
        {
            var input = new TextField("")
            {
                X = 0,
                Y = view.Bounds.Height - 1,
                Width = view.Width
            };

            view.Add(input);
        }
    }
}
