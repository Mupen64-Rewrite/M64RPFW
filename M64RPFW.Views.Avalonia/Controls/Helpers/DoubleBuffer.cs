using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Rendering.Composition;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public abstract class DoubleBuffer<T> : IDisposable where T: class, ICompositionImportableSharedGpuContextImage
{
    private bool _vSync;
    
    private T? _frontBuffer, _backBuffer;
    private Compositor _compositor;
    private ICompositionGpuInterop _interop;
    private CompositionDrawingSurface _target;
    
    private Task? _importTask;

    /// <summary>
    /// Initializes this double-buffer. The derived class is responsible
    /// for initializing the back buffer and passing it here.
    /// </summary>
    /// <param name="backBuffer">The initial back buffer to use</param>
    /// <param name="interop">An <see cref="ICompositionGpuInterop"/> provided by Avalonia</param>
    /// <param name="target">A <see cref="CompositionDrawingSurface"/> to draw to</param>
    /// <param name="vSync">If true, synchronizes updates to compositor updates.</param>
    protected DoubleBuffer(T backBuffer, Compositor compositor, ICompositionGpuInterop interop, CompositionDrawingSurface target, bool vSync = false)
    {
        _vSync = vSync;
        
        _frontBuffer = default;
        _backBuffer = backBuffer;
        _compositor = compositor;
        _interop = interop;
        _target = target;

        _importTask = null;
    }
    
    /// <summary>
    /// Initializes or re-initializes a buffer to match the desired size.
    /// </summary>
    /// <param name="buffer">A reference to the buffer.</param>
    /// <param name="size">The dimensions of the buffer.</param>
    protected abstract void EnsureBufferInitialized([NotNull] ref T? buffer, PixelSize size);

    /// <summary>
    /// Swaps the front and back buffers and begins the process of drawing the front buffer.
    /// </summary>
    /// <param name="size"></param>
    public async Task SwapBuffers(PixelSize size)
    {
        if (_importTask is { IsCompleted: false })
            await _importTask;
        
        (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
        _importTask = ImportAndDisplayFrontBuffer();

        EnsureBufferInitialized(ref _backBuffer, size);
    }

    private async Task ImportAndDisplayFrontBuffer()
    {
        if (_vSync)
            await _compositor.NextVSync();
        var lastImport = _interop.ImportImage(_frontBuffer!);
        await _target.UpdateAsync(lastImport);
    }

    public void Dispose()
    {
        _frontBuffer?.Dispose();
        _backBuffer?.Dispose();
    }
}