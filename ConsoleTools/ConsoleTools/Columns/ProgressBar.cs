using System;
using System.IO;
using System.Text;
using System.Threading;
using ConsoleTools.Columns;

namespace ConsoleTools
{
    public class ProgressBar : IStatusColumn
    {

        private int blockCount = 30;


        public char LeftBorder { get; set; } = '├';
        public char RightBorder { get; set; } = '┤';
        public ConsoleColor ProgressFill { get; set; } = ConsoleColor.White;
        public ConsoleColor ProgressBackground { get; set; } = ConsoleColor.Black;
        public ConsoleColor TextFill { get; set; } = ConsoleColor.Black;
        public ConsoleColor TextBackground { get; set; } = ConsoleColor.White;
        public ConsoleColor BorderColor { get; set; } = ConsoleColor.DarkGray;

        public StatusLine Parent { get; set; }
        
        
        private ConsoleColor? OriginalBackgroundColor => Parent?.OriginalBackgroundColor;
        private ConsoleColor? OriginalForegroundColor => Parent?.OriginalForegroundColor;
        
        public TextWriter ConsoleOut => Parent?.ConsoleOut;

        public int Left { get; set; }

        private int decimalPlaces = 1;
        public int DecimalPlaces
        {
            get => decimalPlaces;
            set
            {
                if (decimalPlaces == value)
                    return;
                decimalPlaces = value;
                FormatString = buildFormatString();
            } 
        }
        public bool ShowPercentage { get; set; } = true;

        private string FormatString { get; set; } = "{0,5:0.0}%";

        private string buildFormatString() {
            if (!ShowPercentage)
                return string.Empty;

            var result = new StringBuilder("{0,");

            result.Append(decimalPlaces < 1 ? "3" : (4 + decimalPlaces).ToString());

            result.Append(":0");

            if (decimalPlaces > 0) {
                result.Append(".");
                result.Append('0', decimalPlaces);
            }

            result.Append("}%");

            return result.ToString();
        }

        private int width = 32;

        public int Width
        {
            get => width;
            set
            {
                width = value;
                blockCount = value - 2;
            }
        }

        public int DesiredWidth => 32;

        public bool isDirty { get; private set; } = true;

        public int MaxValue { get; set; }
        private int value;

        public ProgressBar(int maxValue = 100)
        {
            MaxValue = maxValue;
        }

        public void Step() {
            Interlocked.Exchange(ref value, value + 1);
            isDirty = true;
        }

        public void Reset() {
            Interlocked.Exchange(ref value, 0);
            isDirty = true;
        }

        public void Draw()
        {
            double currentProgress = value / (double)MaxValue;
            int progressBlockCount = (int)(currentProgress * blockCount);
            double percent = currentProgress * 100.0;

            Console.ForegroundColor = BorderColor;
            ConsoleOut.Write(LeftBorder);

            var progressStr = string.Format(FormatString, percent);

            var textStart = (blockCount - 4) / 2;
            var textEnd = textStart + 4;
            var fullProgressString = progressStr.PadLeft(textEnd).PadRight(blockCount);

            Console.BackgroundColor = ProgressFill;
            Console.ForegroundColor = TextFill;
            ConsoleOut.Write(fullProgressString.Substring(0, progressBlockCount));



            Console.BackgroundColor = ProgressBackground;
            Console.ForegroundColor = TextBackground;
            ConsoleOut.Write(fullProgressString.Substring(progressBlockCount));

            Console.BackgroundColor = OriginalBackgroundColor ?? ConsoleColor.Black;

            Console.ForegroundColor = BorderColor;
            ConsoleOut.Write(RightBorder);

            Console.ForegroundColor = OriginalForegroundColor ?? ConsoleColor.White;
            

//            if (currentProgress >= 0.05)
//            {
//                var ticksLeft = (long)(((reportTime - startTime) / currentProgress) * (1.0 - currentProgress));
//                var timeLeft = new TimeSpan(ticksLeft);
//
//                if (animationIndex++ % Parent.FPS == 0)
//                {
//                    showSeparator = !showSeparator;
//                }
//
//                var separator = showSeparator ? ':' : ' ';
//                ConsoleOut.Write(" {0:D2}{3}{1:D2}{3}{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds, separator);
//            }
        }

        public void Dispose()
        {
//            var len = Console.CursorLeft;
//
//            Console.SetCursorPosition(Left, Parent.Top);
//
//            var totalTime = new TimeSpan(reportTime - startTime);
//
//            WriteColoredString("Done", ConsoleColor.Green);
//
//            ConsoleOut.Write(" - {0:D2}:{1:D2}:{2:D2}", totalTime.Hours, totalTime.Minutes, totalTime.Seconds);
//
//            ConsoleOut.WriteLine(new String(' ', Math.Max(len - Console.CursorLeft, 0)));
        }

    }
}
