using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Mupen64PlusRR.Controls.Helpers;
using Mupen64PlusRR.Models.Emulation;
using Mupen64PlusRR.ViewModels.Interfaces;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using static Mupen64PlusRR.Controls.Helpers.SilkGlobals;
using Mutex = System.Threading.Mutex;
using SDL_Window = Silk.NET.SDL.Window;

namespace Mupen64PlusRR.Controls;

public unsafe class VidextControl : NativeControlHost, IVidextSurfaceService
{
    public VidextControl()
    {
    }
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var platHandle = base.CreateNativeControlCore(parent);
        _winHandle = platHandle.Handle;
        
        // HACK: make the window black and clear it
        {
            InitWindow();
            CreateWindow((int) Width, (int) Height, 32);
            
            SDL.GLMakeCurrent(_sdlWin, _sdlGL);
            var gl = GL.GetApi(sym => (IntPtr) SDL.GLGetProcAddress(sym));
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            SDL.GLSwapWindow(_sdlWin);
        }
        
        return platHandle;
    }
    public void InitWindow()
    {
        if (_sdlWin != null)
            return;
        SDL.InitSubSystem(Sdl.InitVideo);
        // needed to let SDL handle OpenGL context management
        SDL.SetHint("SDL_VIDEO_FOREIGN_WINDOW_OPENGL", "1");
        _sdlWin = SDL.CreateWindowFrom((void*) _winHandle);
    }

    public void QuitWindow()
    {
        if (_sdlWin == null)
            return;
        if (_sdlGL != null)
        {
            // clear the screen to black
            var gl = GL.GetApi(sym => (IntPtr) SDL.GLGetProcAddress(sym));
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            SDL.GLSwapWindow(_sdlWin);
            
            SDL.GLDeleteContext(_sdlGL);
        }
        SDL.DestroyWindow(_sdlWin);
        SDL.QuitSubSystem(Sdl.InitVideo);
    }

    public void SetGLAttribute(Mupen64Plus.GLAttribute attr, int value)
    {
        SDL.SetMupenGLAttribute(attr, value);
    }

    public int GetGLAttribute(Mupen64Plus.GLAttribute attr)
    {
        return SDL.GetMupenGLAttribute(attr);
    }

    public void CreateWindow(int width, int height, int bitsPerPixel)
    {
        if (_sdlGL != null)
        {
            SDL.GLMakeCurrent(_sdlWin, null);
            SDL.GLDeleteContext(_sdlGL);
        }

        _sdlGL = SDL.GLCreateContext(_sdlWin);
    }

    public void ResizeWindow(int width, int height)
    {
        if (_sdlGL != null)
        {
            SDL.GLMakeCurrent(_sdlWin, null);
            SDL.GLDeleteContext(_sdlGL);
        }
        _sdlGL = SDL.GLCreateContext(_sdlWin);
    }

    public void MakeCurrent()
    {
        if (_sdlWin == null || _sdlGL == null)
            return;

        SDL.GLMakeCurrent(_sdlWin, _sdlGL);
    }

    public void SwapBuffers()
    {
        if (_sdlWin == null || _sdlGL == null)
            return;
        
        SDL.GLSwapWindow(_sdlWin);
    }

    public IntPtr GetProcAddress(IntPtr strSymbol)
    {
        return (IntPtr) SDL.GLGetProcAddress((byte*) strSymbol);
    }

    public int GetDefaultFramebuffer()
    {
        return 0;
    }
    
    private IntPtr _winHandle;
    private SDL_Window* _sdlWin;
    private void* _sdlGL;
}