using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public class GlBufferGroup : IDisposable
{
    private readonly IGlContext _context;
    private readonly ICompositionImportableOpenGlSharedTexture _texture;
    private readonly uint _fbo;
    private readonly uint _depthRbo;

    ICompositionImportedGpuImage? _import;


    public GlBufferGroup(IOpenGlTextureSharingRenderInterfaceContextFeature glSharing, IGlContext context, PixelSize size, InternalFormat depthFormat)
    {
        _context = context;
        Debug.Assert(!Dispatcher.UIThread.CheckAccess());
        
        _import = null;
        using (context.MakeCurrent())
        {
            var gl = GL.GetApi(context.GlInterface.GetProcAddress);

            _texture = glSharing.CreateSharedTextureForComposition(context, size);
            _fbo = gl.GenFramebuffer();
            _depthRbo = gl.GenRenderbuffer();

            {
                uint prevRbo = (uint) gl.GetInteger(GetPName.RenderbufferBinding);
                uint prevDrawFbo = (uint) gl.GetInteger(GetPName.DrawFramebufferBinding);
                uint prevReadFbo = (uint) gl.GetInteger(GetPName.ReadFramebufferBinding);

                try
                {
                    gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
                    gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);

                    gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                        depthFormat,
                        (uint) size.Width, (uint) size.Height);

                    gl.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                        FramebufferAttachment.ColorAttachment0,
                        TextureTarget.Texture2D, (uint) _texture.TextureId, 0);
                    gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                        FramebufferAttachment.DepthAttachment,
                        RenderbufferTarget.Renderbuffer, _depthRbo);

                    if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
                    {
                        throw new ApplicationException("BAD FRAMEBUFFER IDEA: i don't know, but yeah, that was a bad idea");
                    }
                }
                finally
                {
                    gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, prevRbo);
                    gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, prevDrawFbo);
                    gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, prevReadFbo);
                }
            }
        }

    }

    public uint FBO => _fbo;

    public ICompositionImportedGpuImage Import(ICompositionGpuInterop interop)
    {
        Debug.Assert(Dispatcher.UIThread.CheckAccess());
        
        // If the last import failed, we will need to try again
        if (_import?.ImportCompeted is { Status: TaskStatus.Faulted })
        {
            _import.DisposeAsync();
            _import = null;
        }
        return _import ??= interop.ImportImage(_texture);
    }

    public void Dispose()
    {
        _texture.Dispose();
        using (_context.MakeCurrent())
        {
            var gl = GL.GetApi(_context.GlInterface.GetProcAddress);
            gl.DeleteRenderbuffer(_depthRbo);
            gl.DeleteFramebuffer(_fbo);
        }
    }
}