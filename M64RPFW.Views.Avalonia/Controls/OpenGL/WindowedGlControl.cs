using System;
using Avalonia.Controls;
using Avalonia.Platform;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SDL = Silk.NET.SDL;
using static M64RPFW.Views.Avalonia.Controls.Helpers.SDLHelpers;
using SysDrawing = System.Drawing;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;


public unsafe class WindowedGlControl : NativeControlHost, IOpenGLContextService
{
    private IntPtr _nativeWin;

    private SDL.Window* _sdlWin;
    private void* _sdlCtx;
    private GL _gl;


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
        sdl.GLSwapWindow(_sdlWin);
    }

    public  IntPtr GetProcAddress(IntPtr symbol)
    {
        return (IntPtr) sdl.GLGetProcAddress((byte*) symbol);
    }

    public uint GetDefaultFramebuffer()
    {
        return 0;
    }
}