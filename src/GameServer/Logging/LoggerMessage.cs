using System;
using Terminal.Gui;

namespace GameServer.Logging
{
    public class LoggerMessage : Label
    {
        public LoggerMessage(string text, Color textColor = Color.White, Color backgroundColor = Color.Black) : base(text) 
        {
            this.Text = text;
            SetColor(textColor, backgroundColor);
        }

        public void SetColor(Color textColor, Color backgroundColor) 
        {
            this.TextColor = Application.Driver.MakeAttribute(textColor, backgroundColor);
        }

        public int GetLines() 
        {
            var lines = 0;
            lines += (int)Math.Ceiling(this.Text.Length / (LoggerView.Driver.Clip.Width * 1.0));
            lines += this.Text.ToString().Split('\n').Length - 1;
            return lines;
        }
    }
}