using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace M64RPFW.src.Interfaces
{
    internal interface IDrawingSurfaceProvider
    {
        internal bool IsCreated { get; }
        internal void Create(int width, int height);
        internal void Draw(Array buffer, int width, int height);
    }
}
