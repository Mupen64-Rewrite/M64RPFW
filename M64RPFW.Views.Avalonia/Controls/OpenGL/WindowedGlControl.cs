using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using M64RPFW.Models.Emulation;
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
    
    protected override Size MeasureOverride(Size availableSize)
    {
        return _realSize.ToSize(1);
    }

    public event EventHandler<SkiaRenderEventArgs>? SkiaRender;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var platHandle = base.CreateNativeControlCore(parent);
        platHandle.PlatformWindowSetup();

        if (sdl.InitSubSystem(Sdl.InitVideo) < 0)
            throw new SDLException();

        sdl.SetHint(Sdl.HintVideoForeignWindowOpengl, "1");
        sdl.SetHint(Sdl.HintWindowsEnableMessageloop, "0");
        if (OperatingSystem.IsLinux())
            sdl.SetHint(Sdl.HintVideodriver, "x11");

        _sdlWin = sdl.CreateWindowFrom((void*) platHandle.Handle);
        if (_sdlWin == null)
            throw new SDLException();

        SkiaInit();

        return platHandle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        SkiaQuit();
        sdl.DestroyWindow(_sdlWin);
        sdl.QuitSubSystem(Sdl.InitVideo);

        base.DestroyNativeControlCore(control);
    }

    #region IOpenGLContextService

    public void InitWindow()
    {
        SkiaSetupGL();
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
    }

    public void SetGLAttribute(Mupen64PlusTypes.GLAttribute attr, int value)
    {
        if (attr is Mupen64PlusTypes.GLAttribute.ContextMajorVersion or Mupen64PlusTypes.GLAttribute.ContextMinorVersion or Mupen64PlusTypes.GLAttribute.ContextProfileMask)
            return;
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
        _gl.AttachDebugLogger();
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

    private DateTime _lastpoll = DateTime.Now;

    public void SwapBuffers()
    {
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

    [MemberNotNull(nameof(_skSurface))]
    private void SkiaInitSurface()
    {
        lock (_sizeLock)
        {
            int sampleCount = 0, stencilBits = 0;
            sdl.GLGetAttribute(GLattr.Multisamplesamples, ref sampleCount);
            sdl.GLGetAttribute(GLattr.StencilSize, ref stencilBits);
            
            _skSurface?.Dispose();
            _skSurface = SKSurface.Create(_grContext,
                new GRBackendRenderTarget(
                    _realSize.Width,
                    _realSize.Height,
                    sampleCount,
                    stencilBits,
                    new GRGlFramebufferInfo(0, (uint) GLEnum.Rgba8)),
                SKColorType.Rgba8888);
        }
    }

    private void SkiaSetupGL()
    {
        sdl.GLSetAttribute(GLattr.ContextMajorVersion, 4);
        sdl.GLSetAttribute(GLattr.ContextMinorVersion, 1);
        sdl.GLSetAttribute(GLattr.ContextProfileMask, (int) GLprofile.Compatibility);
        sdl.GLSetAttribute(GLattr.StencilSize, 8);
    }

    private void SkiaInit()
    {
        SkiaSetupGL();
        _skCtx = sdl.GLCreateContext(_sdlWin);
        if (_skCtx == null)
            throw new SDLException();

        // This ensures that even if either SDL or Silk.NET caches pointers
        // under the hood, they won't be somehow mixed up between contexts.
        try
        {
            _skGl = GL.GetApi(sym => (IntPtr) sdl.GLGetProcAddress(sym));
            using (sdl.GLMakeCurrentTemp(_sdlWin, _skCtx))
            {
                if ((_grContext = GRContext.CreateGl()) == null)
                    throw new SystemException("INTERNAL: Skia GRContext.CreateGL failed");
                SkiaInitSurface();
            }
        }
        finally
        {
            SkiaSetupGL();
            sdl.GLMakeCurrent(_sdlWin, null);
        }
    }

    private void SkiaRenderImpl()
    {
        Debug.Assert(_skSurface != null, "_skSurface != null");
        // If no one wants to render we don't need to do anything
        if (SkiaRender == null)
            return;

        using (sdl.GLMakeCurrentTemp(_sdlWin, _skCtx))
        {
            // you need to reinit SKSurface every time the size changes
            if (Interlocked.Exchange(ref _sizeDirty, 0) != 0)
            {
                SkiaInitSurface();
            }
            _grContext.ResetContext();
            SkiaRender(this, new SkiaRenderEventArgs
            {
                Canvas = _skSurface!.Canvas
            });
            
            
            _skSurface!.Flush();
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