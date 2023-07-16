using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public class IndependentGlControlResources : IDisposable
{
    private IOpenGlTextureSharingRenderInterfaceContextFeature _glSharing;
    private IGlContext _context;
    private GlBufferGroup? _front, _middle, _back;
    private int _hasSwapped = 0;
    
    private static InternalFormat CalculateDepthFormat(int depthSize, GlVersion version)
    {
        return depthSize switch
        {
            >= 16 and < 24 => InternalFormat.DepthComponent16,
            >= 24 and < 32 => InternalFormat.DepthComponent24,
            32 => InternalFormat.DepthComponent32,
            _ => version.Type switch
            {
                GlProfileType.OpenGL => InternalFormat.DepthComponent,
                GlProfileType.OpenGLES => InternalFormat.DepthComponent16,
                _ => InternalFormat.DepthComponent16
            }
        };
    }

    public IndependentGlControlResources(IOpenGlTextureSharingRenderInterfaceContextFeature glSharing, GlVersion? preferredVersion = null)
    {
        _glSharing = glSharing;
        _context = glSharing.CreateSharedContext(preferredVersion.HasValue? new[] {preferredVersion.Value} : null)!;
    }

    public IDisposable MakeCurrent()
    {
        return _context.MakeCurrent();
    }

    public IntPtr GetProcAddress(string sym)
    {
        return _context.GlInterface.GetProcAddress(sym);
    }

    public void SwapBuffers(PixelSize size, int depth)
    {
        var gl = GL.GetApi(_context.GlInterface.GetProcAddress);
        gl.Finish();
        _back = Interlocked.Exchange(ref _middle, _back);
        _hasSwapped = 1;
        InitBackBuffer(size, depth);
    }

    public void InitBackBuffer(PixelSize size, int depth)
    {
        _back ??= new GlBufferGroup(_glSharing, _context, size, CalculateDepthFormat(depth, _context.Version));
        var gl = GL.GetApi(_context.GlInterface.GetProcAddress);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _back.FBO);
    }

    public async Task PresentNext(ICompositionGpuInterop interop, CompositionDrawingSurface surface)
    {
        if (Interlocked.Exchange(ref _hasSwapped, 0) != 0)
            _front = Interlocked.Exchange(ref _middle, _front);
        
        if (_front == null)
            return;
        
        var import = _front.Import(interop);
        await surface.UpdateAsync(import);
    }

    public uint? FBO => _back?.FBO;

    public GlVersion ContextVersion => _context.Version;

    public void Dispose()
    {
        _context.Dispose();
        _front?.Dispose();
        _middle?.Dispose();
        _back?.Dispose();
    }
}