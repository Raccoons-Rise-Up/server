using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

using Attribute = Terminal.Gui.Attribute; // Fixes error CS0104	'Attribute' is an ambiguous reference between 'Terminal.Gui.Attribute' and 'System.Attribute'

namespace GameServer
{
    public class Logger
    {
        public static void WorkerThread()
        {
            Application.Init();

            // The main console view
            //var loggerView = new LoggerView();
            //Application.GrabMouse(loggerView);
            //Application.Top.Add(loggerView);

            //CreateInputField(loggerView);

            var colorScheme = new ColorScheme
            {
                Normal = Attribute.Make(Color.White, Color.Black)
            };

            var label = new Label("Hello World")
            {
                ColorScheme = colorScheme,
                X = 0,
                Y = 0,
                Width = 10,
                Height = 1
            };

            //loggerView.Add(label);
            //Application.Refresh();

            var scrollView = new View
            {
                Text = "123",
                X = 0,
                Y = 0,
                Width = 10,
                Height = 10
            };
            scrollView.Add(label);
            Application.Top.Add(scrollView);
            

            Program.StartServer(); // Finished setting up Logger, now start the server

            Application.Run();
        }

        private static void CreateInputField(LoggerView view)
        {
            var input = new TextField("")
            {
                X = 0,
                Y = LoggerView.Driver.Clip.Bottom - 1,
                Width = LoggerView.Driver.Clip.Width
            };

            view.Add(input);
        }
    }
}
