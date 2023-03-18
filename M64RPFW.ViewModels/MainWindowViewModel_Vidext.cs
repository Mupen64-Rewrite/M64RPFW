using M64RPFW.Models.Emulation;
using M64RPFW.Models.Interfaces;
using M64RPFW.Models.Types;

namespace M64RPFW.ViewModels;

using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;
using Error = Mupen64PlusTypes.Error;

public unsafe partial class MainWindowViewModel : IVideoExtensionService
{
    #region Video Extension Functions

    public Error VidextInit()
    {
        try
        {
            _vidextSurfaceService.InitWindow();
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
            _vidextSurfaceService.QuitWindow();
            Resizable = true;
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextListFullscreenModes(Span<Mupen64PlusTypes.Size2D> sizes, ref int len)
    {
        // FUTURE: support fullscreen
        return Error.Unsupported;
    }

    public Error VidextListFullscreenRates(Mupen64PlusTypes.Size2D size, Span<int> output, ref int len)
    {
        // FUTURE: support fullscreen
        return Error.Unsupported;
    }

    public Error VidextSetVideoMode(int width, int height, int bpp, Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags)
    {
        // FUTURE: support fullscreen
        try
        {
            if (mode != Mupen64PlusTypes.VideoMode.Windowed)
                return Error.Unsupported;
            
            Mupen64Plus.Log(LogSources.Vidext, MessageLevel.Info, $"Setting video mode {width}x{height}");
            WindowWidth = width;
            WindowHeight = height + MenuHeight;
            if ((flags & Mupen64PlusTypes.VideoFlags.SupportResizing) == 0)
                Resizable = false;

            _vidextSurfaceService.CreateWindow(width, height, bpp);
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp, Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags)
    {
        return Error.Unsupported;
    }

    public IntPtr VidextGLGetProcAddress(byte* symbol)
    {
        return _vidextSurfaceService.GetProcAddress((IntPtr) symbol);
    }

    public Error VidextGLSetAttr(Mupen64PlusTypes.GLAttribute attr, int value)
    {
        try
        {
            _vidextSurfaceService.SetGLAttribute(attr, value);
            return Error.Success;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error VidextGLGetAttr(Mupen64PlusTypes.GLAttribute attr, out int value)
    {
        try
        {
            value = _vidextSurfaceService.GetGLAttribute(attr);
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
            _vidextSurfaceService.ResizeWindow(width, height);
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
            _vidextSurfaceService.SwapBuffers();
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

    private void OnSizeChanged()
    {
        if (!MupenIsActive)
            return;
        var width = Math.Min((uint) WindowWidth, 65535);
        var height = Math.Min((uint) WindowHeight, 65535);
        Mupen64Plus.CoreStateSet(Mupen64PlusTypes.CoreParam.VideoSize, (width << 16) | height);
    }
}