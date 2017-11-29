using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools.Columns
{
    public interface IStatusColumn: IDisposable
    {

        int Left { get; set; }
        int Width { get; set; }
        bool isDirty { get; }
        void Draw();
        int DesiredWidth { get; }
        StatusLine Parent { get; set; }
    }
}
