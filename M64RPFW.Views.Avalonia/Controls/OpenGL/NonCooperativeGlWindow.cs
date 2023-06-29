using System;
using System.Threading.Tasks;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using M64RPFW.Services;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public class NonCooperativeGlControl : CompositionControl
{

    // Composition objects
    private IGlContext? _glContext;
    private IOpenGlTextureSharingRenderInterfaceContextFeature? _sharingFeature;
    private OpenGlDoubleBuffer? _doubleBuffer;

    // OpenGL objects
    private uint _fbo;
    private uint _depthRbo;
    
    // Option values
    public GlVersion GlVersion { get; set; }
    public bool VSync { get; set; }

    public NonCooperativeGlControl()
    {
        _fbo = _depthRbo = 0;
    }

    protected async override Task InitGpuResources(Compositor compositor, CompositionDrawingSurface surface,
        ICompositionGpuInterop interop)
    {
        _sharingFeature =
            await compositor.GetRenderInterfaceFeature<IOpenGlTextureSharingRenderInterfaceContextFeature>();
        if (!_sharingFeature.CanCreateSharedContext)
            throw new PlatformNotSupportedException("Can't create shared context");

        _glContext = _sharingFeature.CreateSharedContext(new[] { GlVersion })!;

        _doubleBuffer = new OpenGlDoubleBuffer(compositor, interop, surface, _glContext, _sharingFeature, VSync);

        using (_glContext.MakeCurrent())
        {
            var gl = GL.GetApi(_glContext.GlInterface.GetProcAddress);
            _fbo = gl.GenFramebuffer();

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            _depthRbo = gl.GenRenderbuffer();
            gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer, _depthRbo);
        }
    }

    protected override Task FreeGpuResources()
    {
        if (_glContext is not { IsLost: false })
            return Task.CompletedTask;
        
        using (_glContext.MakeCurrent())
        {
            var gl = GL.GetApi(_glContext.GlInterface.GetProcAddress);
            if (_fbo != 0)
            {
                gl.DeleteFramebuffer(_fbo);
                _fbo = 0;
            }

            if (_depthRbo != 0)
            {
                gl.DeleteRenderbuffer(_depthRbo);
                _depthRbo = 0;
            }
        }
        return Task.CompletedTask;
    }
}