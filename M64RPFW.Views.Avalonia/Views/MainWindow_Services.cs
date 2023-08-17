using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : IViewDialogService, ILuaInterfaceService
{

    #region IWindowSizingService

    public WindowSize GetWindowSize()
    {
        return new WindowSize(GlControl.Bounds.Width, GlControl.Bounds.Height);
    }

    bool _isSizedToFit = false;
    private double _glPrevMinWidth, _glPrevMinHeight, _glPrevMaxWidth, _glPrevMaxHeight;

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
            GlControl.MinWidth = GlControl.MaxWidth = size.Width;
            GlControl.MinHeight = GlControl.MaxHeight = size.Height;

            InvalidateMeasure();
        });
    }

    public void UnlockWindowSize()
    {
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
        OpenMovieDialog d = new();
        d.ViewModel.IsEditable = paramsEditable;
        return d.ShowDialog<OpenMovieDialogResult?>(this);
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

    public WindowPoint PointerPosition { get; private set; } = new(0, 0);
    public MouseButtonMask PointerButtons { get; private set; } = 0;
    public event EventHandler<SkiaRenderEventArgs>? OnSkiaRender;
    
    private void SkiaOnRender(object? s, SkiaRenderEventArgs e)
    {
        var canvas = e.Canvas;
        OnSkiaRender?.Invoke(s, e);
    }
    
    private void SkiaOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SkiaOnPointerUpdate(sender, e);
    }
    
    private void SkiaOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        SkiaOnPointerUpdate(sender, e);
    }
    
    private void SkiaOnPointerUpdate(object? sender, PointerEventArgs e)
    {
        var pointerPoint = e.GetCurrentPoint(GlControl);
        var pointerPos = pointerPoint.Position;
        var pointerProps = pointerPoint.Properties;
    
        PointerPosition = new WindowPoint(pointerPos.X, pointerPos.Y);
        PointerButtons =
            (pointerProps.IsLeftButtonPressed ? MouseButtonMask.Primary : 0) |
            (pointerProps.IsMiddleButtonPressed ? MouseButtonMask.Middle : 0) |
            (pointerProps.IsRightButtonPressed ? MouseButtonMask.Secondary : 0);
    }

    #endregion
}