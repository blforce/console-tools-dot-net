using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleTools.Columns;

namespace ConsoleTools
{
    public class StatusLine: IDisposable
    {
        private List<IStatusColumn> columns = new List<IStatusColumn>();
        private readonly Timer timer;

        public readonly ConsoleColor OriginalBackgroundColor;
        public readonly ConsoleColor OriginalForegroundColor;

        public TextWriter ConsoleOut { get; }

        private double _fps = 16.0;

        public double FPS {
            get { return _fps; }
            set {
                _fps = value;
                animationInterval = TimeSpan.FromSeconds(1.0 / value);
            }
        }

        public int Top { get; private set; }

        private TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 16.0);

        public StatusLine() {
            Top = Console.CursorTop;
            timer = new Timer(OnUpdate);

            ConsoleOut = Console.Out;
            OriginalBackgroundColor = Console.BackgroundColor;
            OriginalForegroundColor = Console.ForegroundColor;
        }

        public void Start() => ResetTimer();

        public void AddColumn(IStatusColumn column) {
            lock (timer) {
                column.Parent = this;
                column.Left = columns.Count == 0 ? 1 : columns.Max(x => x.Width + x.Left) + 1;
                columns.Add(column);
            }
        }

        private void OnUpdate(object state) {
            lock (timer) {
                foreach (var column in columns.Where(x => x.isDirty)) {
                    Console.SetCursorPosition(column.Left, Top);
                    column.Draw();
                }
                ConsoleOut.WriteLine();
            }

            ResetTimer();
        }

        public void Dispose() {
            lock (timer) {
                while (columns.Count > 0) {
                    var col = columns.First();
                    columns.Remove(col);
                    col.Dispose();
                }
            }
            ResetTimer();
        }



        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }
    }
}
