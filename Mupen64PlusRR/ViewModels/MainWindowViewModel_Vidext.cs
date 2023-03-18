
using System;
using System.Runtime.InteropServices;
using Mupen64PlusRR.Models.Emulation;
using Mupen64PlusRR.Models.Interfaces;

namespace Mupen64PlusRR.ViewModels;
using PluginType = Mupen64Plus.PluginType;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64Plus.MessageLevel;
using Error = Mupen64Plus.Error;
public unsafe partial class MainWindowViewModel : IVideoExtensionService
{
    #region Video Extension Functions

    public Error VidextInit()
    {
        try
        {
            VidextSurfaceService.InitWindow();
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextQuit()
    {
        try
        {
            VidextSurfaceService.QuitWindow();
            Resizable = true;
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextListFullscreenModes(Span<Mupen64Plus.Size2D> sizes, ref int len)
    {
        // FUTURE: support fullscreen
        return Error.Unsupported;
    }

    public Error VidextListFullscreenRates(Mupen64Plus.Size2D size, Span<int> output, ref int len)
    {
        // FUTURE: support fullscreen
        return Error.Unsupported;
    }

    public Error VidextSetVideoMode(int width, int height, int bpp, Mupen64Plus.VideoMode mode, Mupen64Plus.VideoFlags flags)
    {
        // FUTURE: support fullscreen
        try
        {
            if (mode != Mupen64Plus.VideoMode.Windowed)
                return Error.Unsupported;
            
            Mupen64Plus.Log(LogSources.Vidext, MessageLevel.Info, $"Setting video mode {width}x{height}");
            WindowWidth = width;
            WindowHeight = height + MenuHeight;
            if ((flags & Mupen64Plus.VideoFlags.SupportResizing) == 0)
                Resizable = false;

            VidextSurfaceService.CreateWindow(width, height, bpp);
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp, Mupen64Plus.VideoMode mode, Mupen64Plus.VideoFlags flags)
    {
        return Error.Unsupported;
    }

    public IntPtr VidextGLGetProcAddress(byte* symbol)
    {
        return VidextSurfaceService.GetProcAddress((IntPtr) symbol);
    }

    public Error VidextGLSetAttr(Mupen64Plus.GLAttribute attr, int value)
    {
        try
        {
            VidextSurfaceService.SetGLAttribute(attr, value);
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextGLGetAttr(Mupen64Plus.GLAttribute attr, out int value)
    {
        try
        {
            value = VidextSurfaceService.GetGLAttribute(attr);
            return Error.Success;
        }
        catch (Exception)
        {
            value = 0;
            return Error.Internal;
        }
    }

    public Error VidextResizeWindow(int width, int height)
    {
        try
        {
            VidextSurfaceService.ResizeWindow(width, height);
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextSetCaption(string str)
    {
        // This is absolutely useless!
        return Error.Success;
    }

    public Error VidextToggleFullscreen()
    {
        // FUTURE: support resizing/fullscreen
        return Error.Unsupported;
    }

    public Error VidextSwapBuffers()
    {
        try
        {
            VidextSurfaceService.SwapBuffers();
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public uint VidextGLGetDefaultFramebuffer()
    {
        return 0;
    }

    #endregion

    partial void OnSizeChanged()
    {
        if (!MupenIsActive)
            return;
        uint width = Math.Min((uint) WindowWidth, 65535);
        uint height = Math.Min((uint) WindowHeight, 65535);
        Mupen64Plus.CoreStateSet(Mupen64Plus.CoreParam.VideoSize, (width << 16) | height);
    }
}