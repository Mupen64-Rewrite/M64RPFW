using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.OpenGL;
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
        return new WindowSize(VidextToggle.Width, VidextToggle.Height);
    }

    public void LayoutToFit(WindowSize size)
    {
        VidextToggle.MinWidth = size.Width;
        VidextToggle.MinHeight = size.Height;
        SizeToContent = SizeToContent.WidthAndHeight;
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
        throw new NotImplementedException();
    }

    public void QuitWindow()
    {
        throw new NotImplementedException();
    }

    public void SetGLAttribute(GLAttribute attr, int value)
    {
        if (VidextToggle.IsChildAttached)
            throw new InvalidOperationException("OpenGL is already initialized");
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
            case GLAttribute.SwapControl:
                GlControl.VSync = (value != 0);
                break;
            case GLAttribute.MultisampleBuffers or
                GLAttribute.MultisampleSamples:
                // this is also out of our control
                break;
            case GLAttribute.ContextMajorVersion:
                GlControl.RequestedGlVersionMajor = value;
                break;
            case GLAttribute.ContextMinorVersion:
                GlControl.GlVersionMinor = value;
                break;
            case GLAttribute.ContextProfileMask:
                switch ((GLContextType) value)
                {
                    case GLContextType.Core or GLContextType.Compatibilty:
                        GlControl.GlProfileType = GlProfileType.OpenGL;
                        break;
                    case GLContextType.ES:
                        GlControl.GlProfileType = GlProfileType.OpenGLES;
                        break;
                }
                break;

        }
    }

    public int GetGLAttribute(GLAttribute attr)
    {
        if (_contextHandle == null)
            _contextHandle = GlControl.MakeContextCurrent();
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
                FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, 
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
            GLAttribute.ContextMajorVersion => GlControl.ContextVersion?.Major ?? throw new InvalidOperationException("OpenGL context not init"),
            GLAttribute.ContextMinorVersion => GlControl.ContextVersion?.Minor ?? throw new InvalidOperationException("OpenGL context not init"),
            GLAttribute.ContextProfileMask => (GlControl.ContextVersion?.Type ?? throw new InvalidOperationException("OpenGL context not init")) switch
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
        throw new NotImplementedException();
    }

    public void ResizeWindow(int width, int height)
    {
        throw new NotImplementedException();
    }

    private IDisposable? _contextHandle;

    public void MakeCurrent()
    {
        _contextHandle = GlControl.MakeContextCurrent();
    }

    public void SwapBuffers()
    {
        _contextHandle?.Dispose();
        GlControl.SwapBuffers().Wait();
        _contextHandle = GlControl.MakeContextCurrent();
    }

    public nint GetProcAddress(nint strSymbol)
    {
        string? str = Marshal.PtrToStringAnsi(strSymbol);
        return str != null ? GlControl.GetProcAddress(str) : IntPtr.Zero;
    }

    public int GetDefaultFramebuffer()
    {
        return (int) GlControl.RenderFBO;
    }

    #endregion
}