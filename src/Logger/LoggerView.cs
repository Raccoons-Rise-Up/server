using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace GameServer
{
    public class LoggerView : View
    {
        public LoggerView() : base()
        {
            var colorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Red, Color.Blue), // Labels
                Focus = Application.Driver.MakeAttribute(Color.Red, Color.Blue), // Text Fields

                // These attributes are not needed, however if they do come up for whatever reason
                // they will be visible with vibrant notable colors.
                Disabled = Application.Driver.MakeAttribute(Color.Red, Color.Blue),
                HotFocus = Application.Driver.MakeAttribute(Color.Green, Color.Magenta),
                HotNormal = Application.Driver.MakeAttribute(Color.Cyan, Color.BrightRed)
            };

            this.ColorScheme = colorScheme;
        }

        public override bool ProcessKey(KeyEvent e)
        {
            base.ProcessKey(e);

            if (e.Key == Key.F) { }

            return false;
        }
    }
}
