using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkiaSharp;
using static M64RPFW.Views.Avalonia.Controls.Helpers.SilkGlobals;
using SDL_Window = Silk.NET.SDL.Window;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

/// <summary>
/// Control that displays OpenGL output via an external window. Can also manage a Skia context in tandem.
/// </summary>
public unsafe class WindowedGlControl : NativeControlHost, IOpenGLContextService
{
    private IntPtr _nativeWin;
    private SDL_Window* _sdlWin;
    private void* _emulatorGl;
    
    private void* _skiaGl;
    private GRContext? _grContext;
    private SKSurface? _surface;
    private int _surfaceDirty;

    #region IOpenGLContextService

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var platHandle = base.CreateNativeControlCore(parent);
        _nativeWin = platHandle.Handle;

        // HACK: make the window black and clear it
        {
            InitWindow();
            CreateViewport((int) Width, (int) Height, 32);

            using (SDL.GLMakeCurrentTemp(_sdlWin, _emulatorGl))
            {
                var gl = GL.GetApi(sym => (IntPtr) SDL.GLGetProcAddress(sym));
                gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                gl.Clear(ClearBufferMask.ColorBufferBit);
            }
            SDL.GLSwapWindow(_sdlWin);
        }
        InitSkia();

        return platHandle;
    }

    public void InitWindow()
    {
        if (_sdlWin != null)
            return;
        if (SDL.WasInit(Sdl.InitVideo) == 0)
            SDL.InitSubSystem(Sdl.InitVideo);
        // needed to let SDL handle OpenGL context management
        SDL.SetHint("SDL_VIDEO_FOREIGN_WINDOW_OPENGL", "1");
        // in case SDL wants to use Wayland, force X11 (for now)
        if (OperatingSystem.IsLinux())
            SDL.SetHint("SDL_VIDEODRIVER", "x11");
        _sdlWin = SDL.CreateWindowFrom((void*) _nativeWin);
        SDL.GLSetAttribute(GLattr.Doublebuffer, 1);
    }

    public void QuitWindow()
    {
        if (_sdlWin == null)
            return;
        if (_emulatorGl != null)
        {
            // clear the screen to black
            using (SDL.GLMakeCurrentTemp(_sdlWin, _emulatorGl))
            {
                var gl = GL.GetApi(sym => (IntPtr) SDL.GLGetProcAddress(sym));
                gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                gl.Clear(ClearBufferMask.ColorBufferBit);
            }
            SDL.GLSwapWindow(_sdlWin);

            SDL.GLDeleteContext(_emulatorGl);
            _emulatorGl = null;
        }
        if (_skiaGl != null)
        {
            SDL.GLDeleteContext(_skiaGl);
            _grContext?.Dispose();
            _surface?.Dispose();
            _surfaceDirty = 0;
        }
        SDL.DestroyWindow(_sdlWin);
        _sdlWin = null;

        SDL.QuitSubSystem(Sdl.InitVideo);
    }

    public void SetGLAttribute(Mupen64PlusTypes.GLAttribute attr, int value)
    {
        // We *need* double buffering if we want to render Lua reliably
        if (attr == Mupen64PlusTypes.GLAttribute.DoubleBuffer)
            return;
        SDL.SetMupenGLAttribute(attr, value);
    }

    public int GetGLAttribute(Mupen64PlusTypes.GLAttribute attr)
    {
        return SDL.GetMupenGLAttribute(attr);
    }

    public void CreateViewport(int width, int height, int bitsPerPixel)
    {
        if (_emulatorGl != null)
        {
            SDL.GLMakeCurrent(_sdlWin, null);
            SDL.GLDeleteContext(_emulatorGl);
        }
        _emulatorGl = SDL.GLCreateContext(_sdlWin);
    }

    public void ResizeViewport(int width, int height)
    {
        throw new NotSupportedException("Resizing is not supported");
    }

    public void MakeCurrent()
    {
        if (_sdlWin == null || _emulatorGl == null)
            return;

        SDL.GLMakeCurrent(_sdlWin, _emulatorGl);
    }

    public void SwapBuffers()
    {
        if (_sdlWin == null || _emulatorGl == null)
            return;
        
        TriggerSkiaRender();
        SDL.GLSwapWindow(_sdlWin);
    }

    public IntPtr GetProcAddress(IntPtr strSymbol)
    {
        return (IntPtr) SDL.GLGetProcAddress((byte*) strSymbol);
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
        _surfaceDirty = 1;
    }

    private void InitSurface(GL gl)
    {
        _surface?.Dispose();

        int width, height, samples;
        SDL.GetWindowSizeInPixels(_sdlWin, &width, &height);
        SDL.GLGetAttribute(GLattr.Multisamplesamples, &samples);
            
        gl.Viewport(0, 0, (uint) width, (uint) height);
        _surface = SKSurface.Create(_grContext, 
            new GRBackendRenderTarget(width, height, samples, 0, new GRGlFramebufferInfo(0)), 
            GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    private void InitSkia()
    {
        if (_skiaGl == null)
            _skiaGl = SDL.GLCreateContext(_sdlWin);

        using (SDL.GLMakeCurrentTemp(_sdlWin, _skiaGl))
        {
            var gl = GL.GetApi(sym => (IntPtr) SDL.GLGetProcAddress(sym));
            _grContext = GRContext.CreateGl();

            InitSurface(gl);
        }
    }

    private void TriggerSkiaRender()
    {
        if (_skiaGl == null || _grContext == null || _surface == null)
            return;

        using (SDL.GLMakeCurrentTemp(_sdlWin, _skiaGl))
        {
            if (Interlocked.Exchange(ref _surfaceDirty, 0) != 0)
            {
                var gl = GL.GetApi(sym => (IntPtr) SDL.GLGetProcAddress(sym));
                InitSurface(gl);
            }
            var canvas = _surface.Canvas;
            // TODO: trigger lua drawing
            _surface.Flush();
        }
    }

    #endregion
}