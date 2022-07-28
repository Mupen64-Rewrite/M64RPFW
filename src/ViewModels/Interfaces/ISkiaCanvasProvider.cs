using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFWAvalonia.src.ViewModels.Interfaces
{
    public interface ISkiaCanvasProvider
    {
        public void Invalidate();
        public void Draw(SKImage image, SKSizeI? size = null);
    }
}
