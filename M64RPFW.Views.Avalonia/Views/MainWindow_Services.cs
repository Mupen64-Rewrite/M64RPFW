using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using M64RPFW.Models.Emulation;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Controls;
using M64RPFW.Views.Avalonia.Controls.OpenGL;
using M64RPFW.Views.Avalonia.Helpers;
using Silk.NET.OpenGL;
using SkiaSharp;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : IViewDialogService, ILuaInterfaceService
{

    #region IWindowSizingService

    public WindowSize GetWindowSize()
    {
        return new WindowSize(GlControl.Bounds.Width, GlControl.Bounds.Height);
    }

    bool _isSizedToFit = false;
    double? _oldMaxWidth, _oldMaxHeight;

    public void SizeToFit(WindowSize size)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            GlControl.MinWidth = size.Width;
            GlControl.MinHeight = size.Height;

            SizeToContent = SizeToContent.WidthAndHeight;
            CanResize = false;
            InvalidateMeasure();
        });
    }

    public void UnlockWindowSize()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_oldMaxWidth != null)
                GlControl.MinWidth = _oldMaxWidth.Value;
            if (_oldMaxHeight != null)
                GlControl.MinHeight = _oldMaxHeight.Value;

            SizeToContent = SizeToContent.Manual;
            _isSizedToFit = false;
            CanResize = true;
        });
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