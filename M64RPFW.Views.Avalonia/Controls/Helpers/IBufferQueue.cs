using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Rendering.Composition;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

/// <summary>
/// Generic interface for a class handling the concurrent queuing of buffers.
/// </summary>
/// <typeparam name="TImage">The type of each buffer.</typeparam>
public interface IBufferQueue<out TImage> : IAsyncDisposable where TImage : class, IQueuableImage
{
    /// <summary>
    /// Returns the current buffer.
    /// </summary>
    TImage CurrentBuffer { get; }

    /// <summary>
    /// If set to true, <see cref="SwapBuffers"/> does not return until <see cref="DisplayNext"/> has been called.
    /// </summary>
    bool VSync { get; set; }

    /// <summary>
    /// Flushes the command queue, retrieves a new pending buffer, and sends the texture to be displayed.
    /// </summary>
    /// <param name="size">The size of the viewport</param>
    public Task SwapBuffers(PixelSize size);
}

public interface IQueuableImage : IAsyncDisposable
{
    PixelSize Size { get; }
    
    public void Setup()
    {
    }

    ICompositionImportedGpuImage Import(ICompositionGpuInterop interop);

    public void Reset()
    {
    }
}