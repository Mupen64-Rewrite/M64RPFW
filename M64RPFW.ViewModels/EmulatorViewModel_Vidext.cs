using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using Silk.NET.SDL;

namespace M64RPFW.ViewModels;

using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;
using Error = Mupen64PlusTypes.Error;

public unsafe partial class EmulatorViewModel : IVideoExtensionService
{
    #region Video Extension Functions

    public Error VidextInit()
    {
        try
        {
            _openGlContextService.InitWindow();
            return Error.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Error.Internal;
        }
    }

    public Error VidextQuit()
    {
        try
        {
            _openGlContextService.QuitWindow();
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
            if ((flags & Mupen64PlusTypes.VideoFlags.SupportResizing) == 0)
                Resizable = false;
            
            _dispatcherService.Execute(() =>
            {
                _windowSizingService.LayoutToFit(new WindowSize(width, height));
            });
            _openGlContextService.CreateWindow(width, height, bpp);
            
            _openGlContextService.MakeCurrent();

            return Error.Success;
        }
        catch (Exception e)
        {
            Mupen64Plus.Log(LogSources.Vidext, MessageLevel.Error, $"SetVideoMode threw: {e}");
            return Error.Internal;
        }
    }

    public Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp, Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags)
    {
        return Error.Unsupported;
    }

    public IntPtr VidextGLGetProcAddress(byte* symbol)
    {
        return _openGlContextService.GetProcAddress((IntPtr) symbol);
    }

    public Error VidextGLSetAttr(Mupen64PlusTypes.GLAttribute attr, int value)
    {
        try
        {
            _openGlContextService.SetGLAttribute(attr, value);
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
            value = _openGlContextService.GetGLAttribute(attr);
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
            _openGlContextService.ResizeWindow(width, height);
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
            _openGlContextService.SwapBuffers();
            return Error.Success;
        }
        catch (Exception e)
        {
            Mupen64Plus.Log(LogSources.Vidext, MessageLevel.Error, $"VidextSwapBuffers: {e}");
            return Error.Internal;
        }
    }

    public uint VidextGLGetDefaultFramebuffer()
    {
        return _openGlContextService.GetDefaultFramebuffer();
    }

    #endregion

    public void OnSizeChanged()
    {
        if (!MupenIsActive)
            return;
        var size = _windowSizingService.GetWindowSize();
        var width = Math.Min((uint) size.Width, 65535);
        var height = Math.Min((uint) size.Height, 65535);
        Mupen64Plus.CoreStateSet(Mupen64PlusTypes.CoreParam.VideoSize, (width << 16) | height);
    }
    
    public void ForwardSDLKeyDown(Scancode scancode, Keymod modifiers)
    {
        if (MupenIsActive)
        {
            Mupen64Plus.SendSDLKeyDown(scancode, modifiers);
        }
    }
    
    public void ForwardSDLKeyUp(Scancode scancode, Keymod modifiers)
    {
        if (MupenIsActive)
        {
            Mupen64Plus.SendSDLKeyUp(scancode, modifiers);
        }
    }
}