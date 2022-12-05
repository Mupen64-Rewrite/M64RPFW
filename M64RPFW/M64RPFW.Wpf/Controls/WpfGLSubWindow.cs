using M64RPFW.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

using Size = System.Drawing.Size;

namespace M64RPFW.Wpf.Controls;

/// <summary>
/// Custom WPF control representing an OpenGL window.
/// </summary>
public class WpfGLSubWindow : UserControl
{
    public WpfGLSubWindow() : this(new Size(640, 480))
    {
            
    }

    public WpfGLSubWindow(Size size)
    {
        Loaded += OnLoaded;
        IsVisibleChanged += OnVisibleChanged;
        LayoutUpdated += OnLayoutUpdated;

        MinHeight = size.Height;
        MinWidth = size.Width;

        _windowSize = size;
        _attrMap = new();
        _queueRealize = false;

        Focusable = false;
    }

    public Error SetAttribute(GLAttribute attr, int value)
    {
        if (_glWindow != null)
            return Error.InvalidState;
        _attrMap[attr] = value;
        return Error.Success;
    }

    public Error GetAttribute(GLAttribute attr, ref int value)
    {
        if (_glWindow == null)
        {
            return Error.InvalidState;
        }

        try
        {
            value = _glWindow.GetAttribute(attr);
            return Error.Success;
        }
        catch (NotSupportedException)
        {
            return Error.Unsupported;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error SetVideoMode(Size size, int bitsPerPixel, VideoMode screenMode)
    {
        if (screenMode != VideoMode.Windowed)
        {
            return Error.Unsupported;
        }

        _windowSize = size;
        
        if (!IsLoaded)
            _queueRealize = true;
        else
            InitGLWindow();

        return Error.Success;
    }

    public void MakeCurrent()
    {
        _glWindow?.MakeCurrent();
    }

    public void SwapBuffers()
    {
        _glWindow?.SwapBuffers();
    }

    public void ResizeWindow(Size size)
    {
        _windowSize = size;
        Width = MinWidth = size.Width;
        Height = MinHeight = size.Height;
    }

    public void CloseVideo()
    {
        Console.WriteLine("Video Closing");
        _glWindow?.Dispose();
        _glWindow = null;
    }


    public IntPtr GetProcAddress(string symbol)
    {
        return _glWindow!.GetProcAddress(symbol);
    }

    private void InitGLWindow()
    {
        Window win = Window.GetWindow(this)!;
        _glWindow = new Win32SubWindow(win, _windowSize, _attrMap);

        var winPos = TransformToAncestor(win).Transform(new System.Windows.Point(0, 0));

        var basePos = new System.Drawing.Point
        {
            X = (int) winPos.X + ((int) ActualWidth - _windowSize.Width) / 2,
            Y = (int) winPos.Y + ((int) ActualHeight - _windowSize.Height) / 2
        };
        _glWindow.SetPosition(basePos);
    }

    #region WPF event handlers

    private void OnLoaded(object obj, RoutedEventArgs evt)
    {
        if (_queueRealize && _glWindow != null)
            InitGLWindow();
    }

    private void OnVisibleChanged(object obj, DependencyPropertyChangedEventArgs evt)
    {
        _glWindow?.SetVisible((bool) evt.NewValue);
    }

    private void OnLayoutUpdated(object? obj, EventArgs evt)
    {
        Window? win = Window.GetWindow(this);
        if (win != null)
        {
            var winPos = TransformToAncestor(win).Transform(new System.Windows.Point(0, 0));

            var basePos = new System.Drawing.Point
            {
                X = (int) winPos.X + ((int) ActualWidth - _windowSize.Width) / 2,
                Y = (int) winPos.Y + ((int) ActualHeight - _windowSize.Height) / 2
            };
            _glWindow?.SetPosition(basePos);
        }
    }

    #endregion


    private Dictionary<GLAttribute, int> _attrMap;
    private Size _windowSize;
    private bool _queueRealize;
    private Win32SubWindow? _glWindow;
}