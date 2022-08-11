using System;
using System.Drawing;
using System.Threading;
using Eto.Forms;
using M64RPFW.Views;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Presenters;

/// <summary>
/// Presenter class handling video extension.
/// </summary>
public class VidextPresenter : IVideoExtension
{
    public VidextPresenter(MainView view)
    {
        _view = view;
        _prevContent = null;
    }

    private MainView _view;
    private Control? _prevContent;

    private int _closingFlag = 0;
    internal void NotifyClosing()
    {
        Interlocked.Exchange(ref _closingFlag, 1);
    }

    #region Video Extension API

    public Error Init()
    {
        Console.WriteLine("[VIDEXT] Initializing...");
        Application.Instance.Invoke(delegate
        {
            _prevContent = _view.Content;
            _view.Content = _view.SubWindow;
            _view.ParentWindow.Resizable = false;
            
        });
        return Error.Success;
    }

    public Error Quit()
    {
        Console.WriteLine($"Closing flag: {_closingFlag == 1}");
        if (_closingFlag == 0)
        {
            _view.SubWindow.CloseVideo();
            Application.Instance.Invoke(delegate
            {
                _view.Content = _prevContent;
                _prevContent = null;
                _view.ParentWindow.Resizable = true;

                // Workaround for X11. I know it looks weird, but it works.
                _view.ParentWindow.Size += new Eto.Drawing.Size(10, 10);
                _view.ParentWindow.Size -= new Eto.Drawing.Size(10, 10);
            });
        }
        return Error.Success;
    }

    public (Error err, Size2D[]? modes) ListFullscreenModes(int maxLen)
    {
        return (Error.Unsupported, null);
    }

    public (Error err, int[]? rates) ListFullscreenRates(Size2D size, int maxLen)
    {
        return (Error.Unsupported, null);
    }

    public Error SetVideoMode(int width, int height, int bitsPerPixel, VideoMode mode, VideoFlags flags)
    {
        if (mode == VideoMode.Fullscreen)
            return Error.Unsupported;

        Application.Instance.InvokeAsync(delegate
        {
            _view.MinimumSize = new Eto.Drawing.Size(width, height);
        });
        return _view.SubWindow.SetVideoMode(new Size(width, height), bitsPerPixel, mode, flags);
    }

    public Error SetVideoModeWithRate(int width, int height, int refreshRate, int bitsPerPixel, VideoMode mode, VideoFlags flags)
    {
        return Error.Unsupported;
    }

    public Error ResizeWindow(Size2D size)
    {
        _view.SubWindow.ResizeWindow(new Size { Width = (int) size.uiWidth, Height = (int) size.uiHeight });
        Application.Instance.InvokeAsync(delegate
        {
            _view.MinimumSize = new Eto.Drawing.Size((int) size.uiWidth, (int) size.uiHeight);
        });
        return Error.Success;
    }

    public Error SetCaption(string title)
    {
        _view.Title = $"M64RPFW: {title}";
        return Error.Success;
    }

    public Error ToggleFullScreen()
    {
        return Error.Unsupported;
    }

    public IntPtr GLGetProcAddress(string symbol)
    {
        return _view.SubWindow.GetProcAddress(symbol);
    }

    public Error SetAttribute(GLAttribute attr, int value)
    {
        return _view.SubWindow.SetAttribute(attr, value);
    }

    public Error GetAttribute(GLAttribute attr, ref int value)
    {
        return _view.SubWindow.GetAttribute(attr, ref value);
    }

    public Error SwapBuffers()
    {
        return _view.SubWindow.SwapBuffers();
    }

    public uint GetDefaultFramebuffer()
    {
        return 0;
    }

    #endregion
    
}