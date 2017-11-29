using System;
using System.IO;
using System.Threading;
using ConsoleTools.Columns;

namespace ConsoleTools
{
    public class ProgressBar : IStatusColumn
    {

        private int blockCount = 30;
        private int textStart, textEnd;
        private const string animation = @" ░▒▓█▓▒░";


        public char LeftBorder { get; set; } = '├';
        public char RightBorder { get; set; } = '┤';
        public ConsoleColor ProgressFill { get; set; } = ConsoleColor.White;
        public ConsoleColor ProgressBackground { get; set; } = ConsoleColor.Black;
        public ConsoleColor TextFill { get; set; } = ConsoleColor.Black;
        public ConsoleColor TextBackground { get; set; } = ConsoleColor.White;

        public StatusLine Parent { get; set; }


        private bool disposed = false;
        private int animationIndex = 0;

        private string label = String.Empty;
        private readonly ConsoleColor originalBackgroundColor;
        private readonly ConsoleColor originalForegroundColor;

        private bool showSeparator = true;

        public TextWriter ConsoleOut { get; private set; }
        private int left = -1, percentLeft;

        public int Left {
            get => left;
            set {
                left = value;
                percentLeft = value + blockCount + 2;
            }
        }

        private int width = 32;

        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                blockCount = value + 2;
            }
        }

        public int DesiredWidth => 32;

        public bool isDirty { get; private set; }

        private long startTime, reportTime;
        private int lastPercent = -1;

        public int MaxValue { get; set; }
        private int value = 0;

        public ProgressBar(string label, TextWriter consoleOut = null, int maxValue = 100)
        {
            this.label = label;

            ConsoleOut = consoleOut ?? Console.Out;

            ConsoleOut.Write(this.label);
            Console.Out.Flush();

            if (Left == -1)
                Left = Console.CursorLeft;

            originalBackgroundColor = Console.BackgroundColor;
            originalForegroundColor = Console.ForegroundColor;

            startTime = DateTime.Now.Ticks;
            reportTime = startTime;

            textStart = (blockCount - 4) / 2;
            textEnd = textStart + 4;

            MaxValue = maxValue;
        }

        public void Step() {
            Interlocked.Exchange(ref value, value + 1);
            Interlocked.Exchange(ref reportTime, DateTime.Now.Ticks);
            isDirty = true;
        }

        public void Reset() {
            Interlocked.Exchange(ref value, 0);
            Interlocked.Exchange(ref startTime, DateTime.Now.Ticks);
            Interlocked.Exchange(ref reportTime, startTime);
            isDirty = true;
        }

        public void Draw()
        {
            if (disposed)
                return;

            var top = Parent.Top;

            double currentProgress = value / (double)MaxValue;
            int progressBlockCount = (int)(currentProgress * blockCount);
            int percent = (int)(currentProgress * 100);

            if (percent != lastPercent)
            {
                lastPercent = percent;

                Console.SetCursorPosition(Left, top);
                ConsoleOut.Write(LeftBorder);

                var progressStr = string.Format($"{percent,3}%");

                var fullProgressString = progressStr.PadLeft(textEnd).PadRight(blockCount);

                Console.BackgroundColor = ProgressFill;
                Console.ForegroundColor = TextFill;
                ConsoleOut.Write(fullProgressString.Substring(0, progressBlockCount));



                Console.BackgroundColor = ProgressBackground;
                Console.ForegroundColor = TextBackground;
                ConsoleOut.Write(fullProgressString.Substring(progressBlockCount));

                Console.BackgroundColor = originalBackgroundColor;
                Console.ForegroundColor = originalForegroundColor;

                ConsoleOut.Write(RightBorder);
            }
            else
            {
                Console.SetCursorPosition(percentLeft, top);
            }

            if (currentProgress >= 0.05)
            {
                var ticksLeft = (long)(((reportTime - startTime) / currentProgress) * (1.0 - currentProgress));
                var timeLeft = new TimeSpan(ticksLeft);

                if (animationIndex++ % Parent.FPS == 0)
                {
                    showSeparator = !showSeparator;
                }

                var separator = showSeparator ? ':' : ' ';
                ConsoleOut.Write(" {0:D2}{3}{1:D2}{3}{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds, separator);
            }
            else
            {
                ConsoleOut.Write(" {0}", animation[animationIndex++ % animation.Length]);
            }
        }

        public void Dispose()
        {
            disposed = true;

            var len = Console.CursorLeft;

            Console.SetCursorPosition(Left, Parent.Top);

            var totalTime = new TimeSpan(reportTime - startTime);

            WriteColoredString("Done", ConsoleColor.Green);

            ConsoleOut.Write(" - {0:D2}:{1:D2}:{2:D2}", totalTime.Hours, totalTime.Minutes, totalTime.Seconds);

            ConsoleOut.WriteLine(new String(' ', Math.Max(len - Console.CursorLeft, 0)));
        }

        private void WriteColoredString(string value, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            ConsoleOut.Write(value);
            Console.ForegroundColor = oldColor;
        }

    }
}
