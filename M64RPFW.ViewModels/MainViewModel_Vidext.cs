using M64RPFW.Models.Emulation;
using M64RPFW.Models.Interfaces;

namespace M64RPFW.ViewModels;

public unsafe partial class MainViewModel : IVideoExtensionService
{
    private void WindowDimensionsServiceOnDimensionsChanged((double Width, double Height) obj)
    {
        if (!IsMupenActive)
            return;
        var width = Math.Min((uint)obj.Width, 65535);
        var height = Math.Min((uint)obj.Height, 65535);
        Mupen64Plus.CoreStateSet(Mupen64Plus.CoreParam.VideoSize, (width << 16) | height);
    }
    
    #region Video Extension Functions

    public Mupen64Plus.Error VidextInit()
    {
        try
        {
            _vidextSurfaceService.InitWindow();
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            return Mupen64Plus.Error.Internal;
        }
    }

    public Mupen64Plus.Error VidextQuit()
    {
        try
        {
            _vidextSurfaceService.QuitWindow();
            _windowDimensionsService.IsResizable = true;
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            return Mupen64Plus.Error.Internal;
        }
    }

    public Mupen64Plus.Error VidextListFullscreenModes(Span<Mupen64Plus.Size2D> sizes, ref int len)
    {
        // FUTURE: support fullscreen
        return Mupen64Plus.Error.Unsupported;
    }

    public Mupen64Plus.Error VidextListFullscreenRates(Mupen64Plus.Size2D size, Span<int> output, ref int len)
    {
        // FUTURE: support fullscreen
        return Mupen64Plus.Error.Unsupported;
    }

    public Mupen64Plus.Error VidextSetVideoMode(int width, int height, int bpp, Mupen64Plus.VideoMode mode,
        Mupen64Plus.VideoFlags flags)
    {
        // FUTURE: support fullscreen
        try
        {
            if (mode != Mupen64Plus.VideoMode.Windowed)
                return Mupen64Plus.Error.Unsupported;

            Mupen64Plus.Log(Mupen64Plus.LogSources.Vidext, Mupen64Plus.MessageLevel.Info,
                $"Setting video mode {width}x{height}");
            _windowDimensionsService.Width = width;
            _windowDimensionsService.Height = height + _windowDimensionsService.MenuHeight;
            if ((flags & Mupen64Plus.VideoFlags.SupportResizing) == 0)
                _windowDimensionsService.IsResizable = false;

            _vidextSurfaceService.CreateWindow(width, height, bpp);
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            return Mupen64Plus.Error.Internal;
        }
    }

    public Mupen64Plus.Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp,
        Mupen64Plus.VideoMode mode, Mupen64Plus.VideoFlags flags)
    {
        return Mupen64Plus.Error.Unsupported;
    }

    public nint VidextGLGetProcAddress(byte* symbol)
    {
        return _vidextSurfaceService.GetProcAddress((nint)symbol);
    }

    public Mupen64Plus.Error VidextGLSetAttr(Mupen64Plus.GLAttribute attr, int value)
    {
        try
        {
            _vidextSurfaceService.SetGLAttribute(attr, value);
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            return Mupen64Plus.Error.Internal;
        }
    }

    public Mupen64Plus.Error VidextGLGetAttr(Mupen64Plus.GLAttribute attr, out int value)
    {
        try
        {
            value = _vidextSurfaceService.GetGLAttribute(attr);
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            value = 0;
            return Mupen64Plus.Error.Internal;
        }
    }

    public Mupen64Plus.Error VidextResizeWindow(int width, int height)
    {
        try
        {
            _vidextSurfaceService.ResizeWindow(width, height);
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            return Mupen64Plus.Error.Internal;
        }
    }

    public Mupen64Plus.Error VidextSetCaption(string str)
    {
        // This is absolutely useless!
        return Mupen64Plus.Error.Success;
    }

    public Mupen64Plus.Error VidextToggleFullscreen()
    {
        // TODO: support resizing/fullscreen
        return Mupen64Plus.Error.Unsupported;
    }

    public Mupen64Plus.Error VidextSwapBuffers()
    {
        try
        {
            _vidextSurfaceService.SwapBuffers();
            return Mupen64Plus.Error.Success;
        }
        catch (Exception)
        {
            return Mupen64Plus.Error.Internal;
        }
    }

    public uint VidextGLGetDefaultFramebuffer()
    {
        return 0;
    }

    #endregion
}