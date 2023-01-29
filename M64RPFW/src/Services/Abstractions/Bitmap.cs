using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.src.Services.Abstractions;

internal class Bitmap : IBitmap
{
    private readonly WriteableBitmap _writeableBitmap;
    private readonly Image _image;

    public Bitmap(Image image, int width, int height)
    {
        _image = image;
        _writeableBitmap = new WriteableBitmap(width, height, VisualTreeHelper.GetDpi(image).PixelsPerInchX,
            VisualTreeHelper.GetDpi(image).PixelsPerInchY, PixelFormats.Bgr32, null);
    }

    public void Draw(Array buffer, int width, int height)
    {
        Trace.Assert(Thread.CurrentThread.ManagedThreadId == _image.Dispatcher.Thread.ManagedThreadId);
        
        _writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, width * sizeof(int), 0);
        
        _image.Source = _writeableBitmap;
    }
}