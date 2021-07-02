using Terminal.Gui;

namespace GameServer.Logging
{
    public class LoggerView : View
    {
        public LoggerView() : base()
        {
            DefaultColorScheme();
        }

        public override bool ProcessKey(KeyEvent e)
        {
            base.ProcessKey(e);

            // Reset view back to 'input area' if any key is pressed
            if (Logger.ViewOffset + Logger.GetTotalLines() > LoggerView.Driver.Clip.Height - 1)
            {
                Logger.ViewOffset = -Logger.GetTotalLines() + LoggerView.Driver.Clip.Height - 1;
                Logger.UpdatePositions();
            }

            if (e.Key == Key.Esc) // KEY RESERVED FOR DEBUGGING
            {
                Logger.Log(Application.Driver.Clip.ToString());
                return true;
            }

            if (e.Key == Key.CursorDown)
            {
                if (Logger.CommandHistoryIndex + Logger.CommandHistory.Count + 1 < Logger.CommandHistory.Count)
                {
                    Logger.CommandHistoryIndex++;
                    Logger.Input.Text = Logger.CommandHistory[Logger.CommandHistoryIndex + Logger.CommandHistory.Count];
                }
                return true;
            }

            if (e.Key == Key.CursorUp)
            {
                if (Logger.CommandHistoryIndex + Logger.CommandHistory.Count - 1 >= 0)
                {
                    Logger.CommandHistoryIndex--;
                    Logger.Input.Text = Logger.CommandHistory[Logger.CommandHistory.Count + Logger.CommandHistoryIndex];
                }
                return true;
            }

            if (e.Key == Key.Enter)
            {
                var input = Logger.Input.Text.ToString();
                var cmd = input.Split(' ')[0];

                Logger.Input.Text = "";

                if (cmd == "" || input == "")
                    return false;

                Logger.Log(input);
                Logger.HandleCommands(input);

                Logger.CommandHistory.Add(input);
                return true;
            }

            return false;
        }

        public override bool MouseEvent(MouseEvent e)
        {
            base.MouseEvent(e);

            if (e.Flags == MouseFlags.WheeledUp)
            {
                if (Logger.ViewOffset < 0)
                {
                    Logger.ViewOffset++;
                    Logger.UpdatePositions();
                }
            }

            if (e.Flags == MouseFlags.WheeledDown)
            {
                if (Logger.ViewOffset + Logger.GetTotalLines() > LoggerView.Driver.Clip.Height - 1)
                {
                    Logger.ViewOffset--;
                    Logger.UpdatePositions();
                }
            }

            return true;
        }

        private void DefaultColorScheme()
        {
            var colorScheme = new ColorScheme();
            colorScheme.Normal = Application.Driver.MakeAttribute(Color.White, Color.Black); // Labels
            colorScheme.Focus = Application.Driver.MakeAttribute(Color.White, Color.Black); // Text Fields

            // These attributes are not needed, however if they do come up for whatever reason
            // they will be visible with vibrant notable colors.
            colorScheme.Disabled = Application.Driver.MakeAttribute(Color.Red, Color.Blue);
            colorScheme.HotFocus = Application.Driver.MakeAttribute(Color.Green, Color.Magenta);
            colorScheme.HotNormal = Application.Driver.MakeAttribute(Color.Cyan, Color.BrightRed);

            this.ColorScheme = colorScheme;
        }
    }
}