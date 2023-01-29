using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace M64RPFW.Services.Abstractions;

/// <summary>
///     An <see langword="interface" /> representing a bitmap.
/// </summary>
internal class Bitmap : IBitmap
{
    private readonly Image _image;
    private readonly WriteableBitmap _writeableBitmap;

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