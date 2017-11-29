using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleTools.Columns;

namespace ConsoleTools
{
    public class StatusLine: IDisposable
    {
        private Dictionary<string, IStatusColumn> columns = new Dictionary<string, IStatusColumn>();
        private readonly Timer timer;
        
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
        }

        public void Start() => ResetTimer();

        public void AddColumn(string label, IStatusColumn column) {
            column.Parent = this;
            columns.Add(label, column);
        }

        private void OnUpdate(object state) {
            lock (timer) {
                foreach (var column in columns.Values.Where(x => x.isDirty)) {
                    column.Draw();
                }
            }
        }

        public void Dispose() {
            lock (timer) {
                while (columns.Count > 0) {
                    var col = columns.First();
                    columns.Remove(col.Key);
                    col.Value.Dispose();
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
