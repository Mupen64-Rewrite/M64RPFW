using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.Threading;
using Avalonia.VisualTree;
using M64RPFW.Models.Emulation;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using Silk.NET.OpenGL;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : IWindowSizingService, IViewDialogService, IOpenGLContextService
{
    #region IWindowSizingService

    public WindowSize GetWindowSize()
    {
        return new WindowSize(ContainerPanel.Bounds.Width, ContainerPanel.Bounds.Height);
    }

    bool _isSizedToFit = false;
    double? _oldMaxWidth, _oldMaxHeight;

    public void SizeToFit(WindowSize size, bool sizeGlWindow)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (sizeGlWindow)
            {
                GlControl.WindowSize = new PixelSize((int) size.Width, (int) size.Height);
                GlControl.Width = size.Width;
                GlControl.Height = size.Height;
            }

            if (!_isSizedToFit)
            {
                _oldMaxWidth = ContainerPanel.MaxWidth;
                _oldMaxHeight = ContainerPanel.MaxHeight;
                _isSizedToFit = true;
            }
        
            ContainerPanel.MaxWidth = size.Width;
            ContainerPanel.MaxHeight = size.Height;

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
                ContainerPanel.MaxWidth = _oldMaxWidth.Value;
            if (_oldMaxHeight != null)
                ContainerPanel.MaxHeight = _oldMaxHeight.Value;

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

    #endregion

    #region IOpenGLContextService

    public void InitWindow()
    {
    }

    public void QuitWindow()
    {
        _contextHandle?.Dispose();
        _contextHandle = null;
        
        GlControl.Cleanup();
    }

    public void SetGLAttribute(GLAttribute attr, int value)
    {
        // if (VidextToggle.IsChildAttached)
        //     throw new InvalidOperationException("OpenGL is already initialized");
        switch (attr)
        {
            case GLAttribute.DoubleBuffer or
                GLAttribute.BufferSize or
                GLAttribute.RedSize or
                GLAttribute.BlueSize or
                GLAttribute.GreenSize or
                GLAttribute.AlphaSize:
                // this is out of our control
                break;
            case GLAttribute.DepthSize:
                Dispatcher.UIThread.Invoke(() => GlControl.DepthSize = value);
                break;
            case GLAttribute.SwapControl:
                Dispatcher.UIThread.Invoke(() => GlControl.VSync = value != 0);
                break;
            case GLAttribute.MultisampleBuffers or
                GLAttribute.MultisampleSamples:
                // this is also out of our control
                break;
            case GLAttribute.ContextMajorVersion:
                Dispatcher.UIThread.Invoke(() => GlControl.RequestedGlVersionMajor = value);
                break;
            case GLAttribute.ContextMinorVersion:
                Dispatcher.UIThread.Invoke(() => GlControl.RequestedGlVersionMinor = value);
                break;
            case GLAttribute.ContextProfileMask:
                Dispatcher.UIThread.Invoke(() => GlControl.RequestedGlProfileType = (GLContextType) value switch
                {
                    GLContextType.Core or GLContextType.Compatibilty => GlProfileType.OpenGL,
                    GLContextType.ES => GlProfileType.OpenGLES,
                    _ => GlControl.RequestedGlProfileType
                });
                break;

        }
    }

    public int GetGLAttribute(GLAttribute attr)
    {
        _contextHandle ??= GlControl.MakeContextCurrent();
        if (_contextHandle == null)
            throw new ApplicationException("big screwup here");

        var gl = GL.GetApi(GlControl.GetProcAddress);

        return attr switch
        {
            GLAttribute.DoubleBuffer => 1,
            GLAttribute.BufferSize => gl.GetFramebufferAttachmentParameter(
                                          FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                                          FramebufferAttachmentParameterName.RedSize) +
                                      gl.GetFramebufferAttachmentParameter(
                                          FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                                          FramebufferAttachmentParameterName.GreenSize) +
                                      gl.GetFramebufferAttachmentParameter(
                                          FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                                          FramebufferAttachmentParameterName.BlueSize) +
                                      gl.GetFramebufferAttachmentParameter(
                                          FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                                          FramebufferAttachmentParameterName.AlphaSize),
            GLAttribute.DepthSize => gl.GetFramebufferAttachmentParameter(
                FramebufferTarget.DrawFramebuffer, FramebufferAttachment.DepthAttachment, 
                FramebufferAttachmentParameterName.DepthSize),
            GLAttribute.RedSize => gl.GetFramebufferAttachmentParameter(
                FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                FramebufferAttachmentParameterName.RedSize),
            GLAttribute.GreenSize => gl.GetFramebufferAttachmentParameter(
                FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                FramebufferAttachmentParameterName.GreenSize),
            GLAttribute.BlueSize => gl.GetFramebufferAttachmentParameter(
                FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                FramebufferAttachmentParameterName.BlueSize),
            GLAttribute.AlphaSize => gl.GetFramebufferAttachmentParameter(
                FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
                FramebufferAttachmentParameterName.AlphaSize),
            GLAttribute.SwapControl => GlControl.VSync? 1 : 0,
            GLAttribute.MultisampleBuffers => 0,
            GLAttribute.MultisampleSamples => 0,
            GLAttribute.ContextMajorVersion => GlControl.ContextVersion.Major,
            GLAttribute.ContextMinorVersion => GlControl.ContextVersion.Minor,
            GLAttribute.ContextProfileMask => GlControl.ContextVersion.Type switch
            {
                GlProfileType.OpenGL => (int) GLContextType.Core,
                GlProfileType.OpenGLES => (int) GLContextType.ES,
                _ => throw new ApplicationException("INVALID PROFILE TYPE")
            },
            _ => 0
        };
    }

    public void CreateWindow(int width, int height, int bitsPerPixel)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            GlControl.WindowSize = new PixelSize(width, height);
            await GlControl.Initialize();
        }).Wait();
    }

    public void ResizeWindow(int width, int height)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            GlControl.WindowSize = new PixelSize(width, height);
        });
    }

    private IDisposable? _contextHandle;

    public void MakeCurrent()
    {
        _contextHandle = GlControl.MakeContextCurrent();
    }

    public void SwapBuffers()
    {
        GlControl.SwapBuffers();
    }

    public nint GetProcAddress(nint strSymbol)
    {
        string? str = Marshal.PtrToStringAnsi(strSymbol);
        return str != null ? GlControl.GetProcAddress(str) : IntPtr.Zero;
    }

    public uint GetDefaultFramebuffer()
    {
        return GlControl.RenderFBO;
    }

    #endregion
}