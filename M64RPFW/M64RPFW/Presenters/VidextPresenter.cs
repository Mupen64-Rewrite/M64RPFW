using System;
using System.Threading;
using Eto.Forms;
using Eto.Drawing;
using M64RPFW.Interfaces;
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
        // Minimum size caps the ACTUAL size, not the client size
        // so we get the size of the trim (real size - client size)
        // before adding it to our fixed cap
        _view.MinimumSize = _view.Size - _view.ClientSize + _windowSize;
        // In the future this could potentially hook into M64+ resize
        // functionality.
    }

    private void OnKeyDown(object? sender, KeyEventArgs args)
    {
        SendSDLKeyDown(_keyConv.ProcessKeyEvent(args));
        args.Handled = true;
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs args)
    {
        SendSDLKeyUp(_keyConv.ProcessKeyEvent(args));
        args.Handled = true;
    }
    
    #region Video Extension API

    public Error Init()
    {
        try
        {
            Console.WriteLine("[VIDEXT] Initializing...");
            Application.Instance.Invoke(delegate
            {
                _prevContent = _view.Content;
                _view.Content = _view.SubWindow;
                _decoSize = _view.Size - _view.ClientSize;

                _view.SizeChanged += OnSizeChanged;
                _view.KeyDown += OnKeyDown;
                _view.KeyUp += OnKeyUp;
            });
            return Error.Success;
        }
        catch (Exception e)
        {
            return Error.Internal;
        }
    }

    public Error Quit()
    {
        try
        {
            Console.WriteLine($"Closing flag: {_closingFlag == 1}");
            _view.SubWindow.CloseVideo();
            if (_closingFlag == 0)
            {
                Application.Instance.Invoke(delegate
                {
                    _view.Content = _prevContent;
                    _prevContent = null;
                    _view.ParentWindow.Resizable = true;
                    _view.MinimumSize = new Size(256, 144);

                    _view.SizeChanged -= OnSizeChanged;
                    _view.KeyDown -= OnKeyDown;
                    _view.KeyUp -= OnKeyUp;
                });
            }

            return Error.Success;
        }
        catch (Exception e)
        {
            return Error.Internal;
        }
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
        try
        {
            return _view.SubWindow.SetAttribute(attr, value);
        }
        catch (Exception e)
        {
            return Error.Internal;
        }
    }

    public Error GetAttribute(GLAttribute attr, ref int value)
    {
        try
        {
            return _view.SubWindow.GetAttribute(attr, ref value);
        }
        catch (Exception e)
        {
            return Error.Internal;
        }
    }

    public Error SwapBuffers()
    {
        try
        {
            return _view.SubWindow.SwapBuffers();
        }
        catch (Exception e)
        {
            return Error.Internal;
        }
    }

    public uint GetDefaultFramebuffer()
    {
        return 0;
    }

    #endregion
    
}