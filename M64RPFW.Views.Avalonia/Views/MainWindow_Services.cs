using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Helpers;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : IViewDialogService, ILuaInterfaceService
{

    #region IWindowSizingService

    private bool _isSizedToFit = false;
    private double _glPrevMinWidth, _glPrevMinHeight, _glPrevMaxWidth, _glPrevMaxHeight;

    public WindowSize GetWindowSize()
    {
        return Dispatcher.UIThread.Invoke(() => 
            new WindowSize((int) GlControl.Bounds.Width, (int) GlControl.Bounds.Height));
    }


    public void SizeToFit(WindowSize size)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (!_isSizedToFit)
            {
                _isSizedToFit = true;
                SizeToContent = SizeToContent.WidthAndHeight;
                CanResize = false;

                _glPrevMinWidth = GlControl.MinWidth;
                _glPrevMaxWidth = GlControl.MaxWidth;
                _glPrevMinHeight = GlControl.MinHeight;
                _glPrevMaxHeight = GlControl.MaxHeight;
            }

            // TODO synchronize this somehow
            GlControl.MinWidth = GlControl.MaxWidth = size.Width;
            GlControl.MinHeight = GlControl.MaxHeight = size.Height;
            UpdateLayout();
        });
    }

    public void UnlockWindowSize()
    {
        if (!_isSizedToFit)
            return;
        Dispatcher.UIThread.Invoke(() =>
        {
            GlControl.MinWidth = _glPrevMinWidth;
            GlControl.MaxWidth = _glPrevMaxWidth;
            GlControl.MinHeight = _glPrevMinHeight;
            GlControl.MaxHeight = _glPrevMaxHeight;

            SizeToContent = SizeToContent.Manual;
            _isSizedToFit = false;
            CanResize = true;
        });
    }

    public IntPtr WindowHandle => TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    public WindowSystemID WindowSystemID
    {
        get
        {
            return TryGetPlatformHandle()?.HandleDescriptor switch
            {
                "HWND" => WindowSystemID.Windows,
                // "NSWindow" => WindowSystemID.Cocoa,
                "XID" => WindowSystemID.X11,
                _ => throw new NotSupportedException("Avalonia does not support platform handles here")
            };
        }
    }

    #endregion

    #region IViewDialogService

    public Task ShowSettingsDialog()
    {
        SettingsDialog d = new();
        return d.ShowDialog(this);
    }

    public Task ShowAdvancedSettingsDialog()
    {
        AdvancedSettingsDialog d = new();
        return d.ShowDialog(this);
    }

    public Task<OpenMovieDialogResult?> ShowOpenMovieDialog(bool paramsEditable)
    {
        var d = new OpenMovieDialog();
        d.ViewModel.IsEditable = paramsEditable;
        return d.ShowDialog<OpenMovieDialogResult?>(this);
    }

    public Task<StartEncoderDialogResult?> ShowStartEncoderDialog()
    {
        var d = new StartEncoderDialog();
        return d.ShowDialog<StartEncoderDialogResult?>(this);

    }

    public Task ShowExceptionDialog(Exception e, string? msg = null)
    {
        ExceptionDialog d = new ExceptionDialog
        {
            Detail = e.ToString(),
            Message = msg ?? "Error occurred"
        };
        return d.ShowDialog(this);
    }

    #endregion

    #region ILuaInterfaceService

    private INativeWindowMouseHandler _mouseHandler;

    public WindowPoint PointerPosition => _mouseHandler.Position.ToWindowPoint();
    public MouseButtonMask PointerButtons => _mouseHandler.ButtonMask;
    public event EventHandler<SkiaRenderEventArgs>? OnSkiaRender;

    private void GlControl_OnSkiaRender(object? s, SkiaRenderEventArgs e)
    {
        var canvas = e.Canvas;
        OnSkiaRender?.Invoke(s, e);
    }

    #endregion
}