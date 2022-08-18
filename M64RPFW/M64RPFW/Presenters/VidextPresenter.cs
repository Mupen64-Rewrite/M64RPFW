using System;
using System.Threading;
using Eto.Forms;
using Eto.Drawing;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Presenters.Helpers;
using M64RPFW.Views;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Presenters;

/// <summary>
/// Presenter class handling video extension.
/// </summary>
internal class VidextPresenter : IVideoExtension
{
    public VidextPresenter(MainView view)
    {
        _view = view;
        _prevContent = null;
        _keyConv = new SDLKeyConverter();
    }

    private MainView _view;
    private Control? _prevContent;
    private Size _decoSize;
    private Size _windowSize;
    private SDLKeyConverter _keyConv;

    private int _closingFlag = 0;
    internal void NotifyClosing()
    {
        Interlocked.Exchange(ref _closingFlag, 1);
    }

    private void OnSizeChanged(object? sender, EventArgs args)
    {
        _view.MinimumSize = _view.Size - _view.ClientSize + _windowSize;
    }

    private void OnKeyDown(object? sender, KeyEventArgs args)
    {
        SendSDLKeyDown(_keyConv.ProcessKeyEvent(args));
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs args)
    {
        SendSDLKeyUp(_keyConv.ProcessKeyEvent(args));
    }
    
    #region Video Extension API

    public Error Init()
    {
        Console.WriteLine("[VIDEXT] Initializing...");
        Application.Instance.Invoke(delegate
        {
            _prevContent = _view.Content;
            _view.Content = _view.SubWindow;
            _decoSize = _view.Size - _view.ClientSize;

            _view.SizeChanged += OnSizeChanged;
            ViewInitHelpers.RegisterWindowKeyHandlers(_view.ParentWindow, OnKeyDown, OnKeyUp);
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
                _view.MinimumSize = new Size(256, 144);

                _view.SizeChanged -= OnSizeChanged;
                _view.SubWindow.KeyDown -= OnKeyDown;
                _view.SubWindow.KeyUp -= OnKeyUp;
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

        _windowSize = new Size(width, height);
        
        Application.Instance.InvokeAsync(delegate
        {
            _view.ClientSize = _windowSize;
            
            // Minimum size caps the real size, so we need to know how much
            // the decorations contribute to size (fixed amount)
            _view.MinimumSize = _view.Size - _view.ClientSize + _windowSize;
            _view.SizeChanged += OnSizeChanged;
        });
        return _view.SubWindow.SetVideoMode(new System.Drawing.Size(width, height), bitsPerPixel, mode, flags);
    }

    public Error SetVideoModeWithRate(int width, int height, int refreshRate, int bitsPerPixel, VideoMode mode, VideoFlags flags)
    {
        return Error.Unsupported;
    }

    public Error ResizeWindow(Size2D size)
    {
        _view.SubWindow.ResizeWindow(new System.Drawing.Size { Width = (int) size.uiWidth, Height = (int) size.uiHeight });
        Application.Instance.InvokeAsync(delegate
        {
            _view.MinimumSize = new Size((int) size.uiWidth, (int) size.uiHeight);
        });
        return Error.Success;
    }

    public Error SetCaption(string title)
    {
        // They can't get the title back, so we can just
        // pretend we set the title. Not like we care here.
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