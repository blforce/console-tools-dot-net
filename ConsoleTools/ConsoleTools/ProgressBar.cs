using System;
using System.IO;
using System.Threading;

namespace ConsoleTools
{
    public class ProgressBar : IDisposable, IProgress<double>
    {

        private const int blockCount = 30;
        private int textStart, textEnd;
        private const double fps = 16.0;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / fps);
        private const string animation = @" ░▒▓█▓▒░";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = String.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        private string label = String.Empty;
        private ConsoleColor originalBackgroundColor;
        private ConsoleColor originalForegroundColor;

        private bool showSeparator = true;

        public TextWriter ConsoleOut { get; private set; }
        private int top, left = -1, percentLeft;

        public int Left {
            get => left;
            set {
                left = value;
                percentLeft = value + blockCount;
            }
        }

        private long startTime, reportTime;
        private int lastPercent = -1;

        public ProgressBar(string label, TextWriter consoleOut = null)
        {
            this.label = label;

            ConsoleOut = consoleOut ?? Console.Out;

            ConsoleOut.Write(this.label);
            Console.Out.Flush();

            if (Left == -1)
                Left = Console.CursorLeft;

            top = Console.CursorTop;

            originalBackgroundColor = Console.BackgroundColor;
            originalForegroundColor = Console.ForegroundColor;

            startTime = DateTime.Now.Ticks;
            reportTime = startTime;

            textStart = (blockCount - 4) / 2;
            textEnd = textStart + 4;

            timer = new Timer(UpdateProgress);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
            Interlocked.Exchange(ref reportTime, DateTime.Now.Ticks);
        }

        private void UpdateProgress(object state)
        {
            lock (timer)
            {
                if (disposed)
                    return;

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent = (int)(currentProgress * 100);

                if (percent != lastPercent)
                {
                    lastPercent = percent;

                    Console.SetCursorPosition(Left, top);

                    var progressStr = string.Format($"{percent,3}%");

                    var fullProgressString = progressStr.PadLeft(textEnd).PadRight(blockCount);

                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                    ConsoleOut.Write(fullProgressString.Substring(0, progressBlockCount));



                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleOut.Write(fullProgressString.Substring(progressBlockCount));

                    Console.BackgroundColor = originalBackgroundColor;
                    Console.ForegroundColor = originalForegroundColor;
                }
                else
                {
                    Console.SetCursorPosition(percentLeft, top);
                }

                if (currentProgress >= 0.05)
                {
                    var ticksLeft = (long)(((reportTime - startTime) / currentProgress) * (1.0 - currentProgress));
                    var timeLeft = new TimeSpan(ticksLeft);

                    if (animationIndex++ % fps == 0)
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

                ResetTimer();
            }
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;

                var len = Console.CursorLeft;

                Console.SetCursorPosition(Left, top);

                var totalTime = new TimeSpan(reportTime - startTime);

                WriteColoredString("Done", ConsoleColor.Green);

                ConsoleOut.Write(" - {0:D2}:{1:D2}:{2:D2}", totalTime.Hours, totalTime.Minutes, totalTime.Seconds);

                ConsoleOut.WriteLine(new String(' ', Math.Max(len - Console.CursorLeft, 0)));
            }
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
