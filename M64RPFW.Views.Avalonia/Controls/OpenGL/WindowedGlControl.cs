using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkiaSharp;
using SDL = Silk.NET.SDL;
using static M64RPFW.Views.Avalonia.Controls.Helpers.SDLHelpers;
using SysDrawing = System.Drawing;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public unsafe class WindowedGlControl : NativeControlHost, IOpenGLContextService
{
    private IntPtr _nativeWin;

    private SDL.Window* _sdlWin;
    private void* _sdlCtx;
    private GL _gl = null!;

    private readonly object _sizeLock = new();
    private PixelSize _realSize = default;
    private int _sizeDirty = 0;

    private void* _skCtx;
    private GL _skGl = null!;
    private GRContext _grContext = null!;
    private SKSurface? _skSurface;


    public event EventHandler<SkiaRenderEventArgs>? SkiaRender;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var platHandle = base.CreateNativeControlCore(parent);
        _nativeWin = platHandle.Handle;

        if (sdl.InitSubSystem(Sdl.InitVideo) < 0)
            throw new SDLException();

        sdl.SetHint(Sdl.HintVideoForeignWindowOpengl, "1");
        if (OperatingSystem.IsLinux())
            sdl.SetHint(Sdl.HintVideodriver, "x11");

        _sdlWin = sdl.CreateWindowFrom((void*) _nativeWin);
        if (_sdlWin == null)
            throw new SDLException();

        return platHandle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        sdl.DestroyWindow(_sdlWin);
        sdl.QuitSubSystem(Sdl.InitVideo);

        base.DestroyNativeControlCore(control);
    }

    #region IOpenGLContextService

    public void InitWindow()
    {
    }

    public void QuitWindow()
    {
        if (_sdlCtx != null)
        {
            using (sdl.GLMakeCurrentTemp(_sdlWin, _sdlCtx))
            {
                _gl.ClearColor(SysDrawing.Color.Black);
                _gl.Clear(ClearBufferMask.ColorBufferBit);
                sdl.GLSwapWindow(_sdlWin);
            }

            sdl.GLMakeCurrent(_sdlWin, null);
            sdl.GLDeleteContext(_sdlCtx);
        }

        SkiaQuit();
    }

    public void SetGLAttribute(Mupen64PlusTypes.GLAttribute attr, int value)
    {
        sdl.SetMupenGLAttribute(attr, value);
    }

    public int GetGLAttribute(Mupen64PlusTypes.GLAttribute attr)
    {
        return sdl.GetMupenGLAttribute(attr);
    }

    public void CreateViewport(int width, int height, int bitsPerPixel)
    {
        _sdlCtx = sdl.GLCreateContext(_sdlWin);
        if (_sdlCtx == null)
            throw new SDLException();

        _gl = GL.GetApi(sym => (IntPtr) sdl.GLGetProcAddress(sym));

        SkiaInit();
    }

    public void ResizeViewport(int width, int height)
    {
        throw new NotSupportedException("Direct resizing is not supported because Lua demands it.");
    }

    public void MakeCurrent()
    {
        if (_sdlCtx == null)
            throw new SystemException("INTERNAL: _sdlCtx == null");

        if (sdl.GLMakeCurrent(_sdlWin, _sdlCtx) < 0)
            throw new SDLException();
    }

    public void SwapBuffers()
    {
        _gl.Flush();
        SkiaRenderImpl();
        sdl.GLSwapWindow(_sdlWin);
    }

    public IntPtr GetProcAddress(IntPtr symbol)
    {
        return (IntPtr) sdl.GLGetProcAddress((byte*) symbol);
    }

    public uint GetDefaultFramebuffer()
    {
        return 0;
    }

    #endregion

    #region Skia stuff

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var baseSize = e.NewSize;

        lock (_sizeLock)
            _realSize = new PixelSize((int) (baseSize.Width * scaling), (int) (baseSize.Height * scaling));

        _sizeDirty = 1;
    }

    private SKSurface SkiaInitSurface()
    {
        int msaaSamples = 0, stencilBits = 0;
        sdl.GLGetAttribute(GLattr.Multisamplesamples, ref msaaSamples);
        sdl.GLGetAttribute(GLattr.StencilSize, ref stencilBits);

        SKSurface surface;
        lock (_sizeLock)
        {
            surface = SKSurface.Create(_grContext,
                new GRBackendRenderTarget(
                    _realSize.Width,
                    _realSize.Height,
                    msaaSamples,
                    stencilBits,
                    new GRGlFramebufferInfo(0, (uint) GLEnum.Rgba8)),
                SKColorType.Rgba8888);
        }

        return surface;
    }

    private void SkiaInit()
    {
        _skCtx = sdl.GLCreateContext(_sdlWin);
        if (_skCtx == null)
            throw new SDLException();

        // This ensures that even if either SDL or Silk.NET caches pointers
        // under the hood, they won't be somehow mixed up between contexts.
        _skGl = GL.GetApi(sym => (IntPtr) sdl.GLGetProcAddress(sym));
        using (sdl.GLMakeCurrentTemp(_sdlWin, _skCtx))
        {
            if ((_grContext = GRContext.CreateGl()) == null)
                throw new SystemException("INTERNAL: Skia GRContext.CreateGL failed");
            _skSurface = SkiaInitSurface();
        }
    }

    private void SkiaRenderImpl()
    {
        // If no one wants to render we don't need to do anything
        if (SkiaRender == null)
            return;

        using (sdl.GLMakeCurrentTemp(_sdlWin, _skCtx))
        {
            // you need to reinit SKSurface every time the size changes
            if (Interlocked.Exchange(ref _sizeDirty, 0) != 0)
            {
                _skSurface?.Dispose();
                _skSurface = SkiaInitSurface();
            }
            SkiaRender(this, new SkiaRenderEventArgs
            {
                Canvas = _skSurface!.Canvas
            });
            
            _skSurface.Flush();
        }
    }

    private void SkiaQuit()
    {
        _skSurface?.Dispose();
        _grContext.Dispose();

        sdl.GLDeleteContext(_skCtx);
    }

    #endregion
}