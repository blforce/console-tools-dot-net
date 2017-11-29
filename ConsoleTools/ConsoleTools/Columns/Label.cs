using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools.Columns
{
    public class Label: IStatusColumn {

        private string content = string.Empty;

        public string Content
        {
            get => content;
            set
            {
                if (content == value)
                    return;
                content = value;
                isDirty = true;
            }
        }

        public void Dispose() {
            
        }

        public ConsoleColor Foreground { get; set; } = ConsoleColor.DarkGray;
        public ConsoleColor Background { get; set; } = ConsoleColor.Black;

        public int Left { get; set; }
        public int Width { get; set; }
        public bool isDirty { get; private set; }
        public void Draw() {
            Console.ForegroundColor = Foreground;
            Console.BackgroundColor = Background;

            if (Width == -1) {
                Parent.ConsoleOut.Write(Content.PadRight(Console.BufferWidth - Left));
            }
            else {
                Parent.ConsoleOut.Write(Content.Substring(0, Math.Min(Width, Content.Length)).PadRight(Width));
            }

            Console.ForegroundColor = Parent.OriginalForegroundColor;
            Console.BackgroundColor = Parent.OriginalBackgroundColor;
        }

        public int DesiredWidth { get; } = 32;
        public StatusLine Parent { get; set; }

    }
}
