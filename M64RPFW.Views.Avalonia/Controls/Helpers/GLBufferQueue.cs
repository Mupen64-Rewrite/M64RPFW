using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

/// <summary>
/// Enqueues and displays OpenGL buffers.
/// </summary>
public class GLBufferQueue : IBufferQueue<GLQueuableImage>
{
    private ConcurrentQueue<GLQueuableImage> _pendingBuffers;
    private GLQueuableImage? _currentBuffer;
    private int _disposing;

    private Compositor _compositor;
    private ICompositionGpuInterop _interop;
    private CompositionDrawingSurface _target;
    private IOpenGlTextureSharingRenderInterfaceContextFeature _glSharing;
    private IGlContext _glContext;


    public GLBufferQueue(Compositor compositor, ICompositionGpuInterop interop, CompositionDrawingSurface target,
        IOpenGlTextureSharingRenderInterfaceContextFeature glSharing, IGlContext glContext)
    {
        _compositor = compositor;
        _interop = interop;
        _target = target;
        _glSharing = glSharing;
        _glContext = glContext;
        _pendingBuffers = new ConcurrentQueue<GLQueuableImage>();
        _currentBuffer = null;
        _disposing = 0;
    }

    public GLQueuableImage CurrentBuffer
    {
        get
        {
            if (_currentBuffer == null)
                throw new InvalidOperationException($"Call {nameof(SwapBuffers)} to initialize the current buffer.");
            return _currentBuffer;
        }
    }

    public bool VSync { get; set; }

    async Task DisplayNext(GLQueuableImage img, PixelSize size)
    {
        var import = img.Import(_interop);
        await import.ImportCompeted;

        async void Continuation()
        {
            await _target.UpdateAsync(import);
            _compositor.RequestCommitAsync();

            if (img.Size == size && _disposing != 0)
                _pendingBuffers.Enqueue(img);
            else
                img.DisposeAsync();
        }
        Continuation();
    }
#pragma warning disable CS4014
    public async Task SwapBuffers(PixelSize size)
    {
        
        if (_currentBuffer != null)
        {
            var copyCurrentBuffer = _currentBuffer;
            Dispatcher.UIThread.InvokeAsync(async () => await DisplayNext(copyCurrentBuffer, size));
            _currentBuffer = null;
        }

        // pull new buffer from incoming queue
        if (_disposing != 0)
        {
            ; // absolutely nothing
        }
        else if (_pendingBuffers.Count <= 3)
        {
            _currentBuffer = new GLQueuableImage(size, _glSharing, _glContext);
        }
        else
        {
            while (_pendingBuffers.TryDequeue(out _currentBuffer) && _currentBuffer.Size != size)
            {
                ; // nothing
            }
        }
    }
#pragma warning restore CS4014

    public async ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref _disposing, 1);
        var waitList = new List<Task>();
        // queue all into a list, then wait on all of them simultaneously
        if (_currentBuffer != null)
            waitList.Add(_currentBuffer.DisposeAsync().AsTask());
        while (_pendingBuffers.Count > 0)
        {
            if (_pendingBuffers.TryDequeue(out var buf))
            {
                waitList.Add(buf.DisposeAsync().AsTask());
            }
        }

        await Task.WhenAll(waitList);
    }
}