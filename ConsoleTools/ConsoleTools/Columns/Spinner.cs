using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools.Columns
{
    public class Spinner: IStatusColumn
    {

        public static class SpinnerAnimations {

            public static string Blink = @" ░▒▓█▓▒░";

        }

        public string Animation { get; set; } = SpinnerAnimations.Blink;
        private int animationIndex;

        public int Left { get; set; }
        public int Width { get; set; } = 1;
        public bool isDirty => true;

        public void Draw() {
            Parent.ConsoleOut.Write(Animation[animationIndex++ % Animation.Length]);
        }

        public int DesiredWidth { get; }
        public StatusLine Parent { get; set; }
        public void Dispose() {}

    }
}
