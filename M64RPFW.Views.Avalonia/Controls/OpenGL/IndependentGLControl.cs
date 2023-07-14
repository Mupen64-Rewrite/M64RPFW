using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

/// <summary>
/// A control whose contents are controlled via an external OpenGL render loop.
/// </summary>
public class IndependentGLControl : CompositionControl
{
    // Composition objects
    private IGlContext? _glContext;
    private IOpenGlTextureSharingRenderInterfaceContextFeature? _sharingFeature;
    private GLBufferQueue? _bufferQueue;

    // OpenGL objects
    private uint _renderFbo;
    private uint _depthRbo;

    // Option variables
    private int _requestedGlVersionMajor;
    private int _requestedGlVersionMinor;
    private GlProfileType _requestedGlProfileType;
    private bool _vSync;
    private int _depthSize = 0;

    public static readonly DirectProperty<IndependentGLControl, int> RequestedGlVersionMajorProperty =
        AvaloniaProperty.RegisterDirect<IndependentGLControl, int>(nameof(RequestedGlVersionMajor), c => c._requestedGlVersionMajor,
            (c, value) => c._requestedGlVersionMajor = value);

    public static readonly DirectProperty<IndependentGLControl, int> RequestedGlVersionMinorProperty =
        AvaloniaProperty.RegisterDirect<IndependentGLControl, int>(nameof(RequestedGlVersionMinor), c => c._requestedGlVersionMinor,
            (c, value) => c._requestedGlVersionMinor = value);

    public static readonly DirectProperty<IndependentGLControl, GlProfileType> RequestedGlProfileTypeProperty =
        AvaloniaProperty.RegisterDirect<IndependentGLControl, GlProfileType>(nameof(RequestedGlProfileType),
            c => c._requestedGlProfileType, (c, value) => c._requestedGlProfileType = value);

    public static readonly DirectProperty<IndependentGLControl, bool> VSyncProperty =
        AvaloniaProperty.RegisterDirect<IndependentGLControl, bool>(nameof(VSync), c => c._vSync,
            (c, value) => c._vSync = value);

    public static readonly DirectProperty<IndependentGLControl, int> DepthSizeProperty = AvaloniaProperty.RegisterDirect<IndependentGLControl, int>(nameof(DepthSize), c => c._depthSize, (c, value) => c._depthSize = value);

    // Option values

    public GlVersion GlVersion => new(_requestedGlProfileType, RequestedGlVersionMajor, RequestedGlVersionMinor);

    public bool VSync
    {
        get => _vSync;
        set => SetAndRaise(VSyncProperty, ref _vSync, value);
    }

    public int RequestedGlVersionMajor
    {
        get => _requestedGlVersionMajor;
        set => SetAndRaise(RequestedGlVersionMajorProperty, ref _requestedGlVersionMajor, value);
    }

    public int RequestedGlVersionMinor
    {
        get => _requestedGlVersionMinor;
        set => SetAndRaise(RequestedGlVersionMinorProperty, ref _requestedGlVersionMinor, value);
    }

    public GlProfileType RequestedGlProfileType
    {
        get => _requestedGlProfileType;
        set => SetAndRaise(RequestedGlProfileTypeProperty, ref _requestedGlProfileType, value);
    }

    public int DepthSize
    {
        get => _depthSize;
        set => SetAndRaise(DepthSizeProperty, ref _depthSize, value);
    }

    public IndependentGLControl()
    {

        _renderFbo = _depthRbo = 0;
    }

    private PixelSize ComputePixelSize(Size size)
    {
        double scaling = VisualRoot!.RenderScaling;
        return new PixelSize(Math.Max(1, (int) (size.Width * scaling)), Math.Max(1, (int) (size.Height * scaling)));
    }

    /// <inheritdoc/>
    protected override async Task InitGpuResources(Compositor compositor, CompositionDrawingSurface surface,
        ICompositionGpuInterop interop)
    {
        _sharingFeature =
            await compositor.GetRenderInterfaceFeature<IOpenGlTextureSharingRenderInterfaceContextFeature>();
        if (!_sharingFeature.CanCreateSharedContext)
            throw new PlatformNotSupportedException("Can't create shared context");

        _glContext = _sharingFeature.CreateSharedContext(new[]
                     {
                         GlVersion
                     }) ??
                     throw new ApplicationException("Couldn't create shared context");

        _bufferQueue = new GLBufferQueue(compositor, interop, surface, _sharingFeature, _glContext);

        using (_glContext.MakeCurrent())
        {
            var gl = GL.GetApi(_glContext.GlInterface.GetProcAddress);
            _renderFbo = gl.GenFramebuffer();
        }

        await SwapBuffers();
    }

    /// <inheritdoc/>
    protected async override Task FreeGpuResources()
    {
        if (_glContext == null)
            return;
        if (_bufferQueue != null)
        {
            await _bufferQueue.DisposeAsync();
            _bufferQueue = null;
        }
    }

    public nint GetProcAddress(string sym) =>
        _glContext == null ? IntPtr.Zero : _glContext.GlInterface.GetProcAddress(sym);

    public IDisposable? MakeContextCurrent()
    {
        return _glContext?.MakeCurrent();
    }

    private void InitGLBuffers(GL gl, GLQueuableImage buffer)
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _renderFbo);
        gl.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferParameterName.Width, WindowSize.Width);
        gl.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferParameterName.Height, WindowSize.Height);
        {
            var oldRenderbuffer = (uint) gl.GetInteger(GLEnum.Renderbuffer);
            var depthFormat = _depthSize switch
            {
                < 16 => _glContext!.Version.Type == GlProfileType.OpenGLES
                    ? InternalFormat.DepthComponent16
                    : InternalFormat.DepthComponent,
                >= 16 and < 24 => InternalFormat.DepthComponent16,
                >= 24 and < 32 => InternalFormat.DepthComponent24,
                >= 32 => InternalFormat.DepthComponent32,
            };

            try
            {
                if (_depthRbo != 0)
                {
                    gl.DeleteRenderbuffer(_depthRbo);
                }

                _depthRbo = gl.GenRenderbuffer();
                gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);
                gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depthFormat,
                    (uint) WindowSize.Width, (uint) WindowSize.Height);
                gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRbo);
            }
            finally
            {
                gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, oldRenderbuffer);
            }
        }

        // Attach the new buffer to the FBO, and make sure it's working
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Texture2D,
            buffer.TextureObject, 0);
        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new ApplicationException("Framebuffer dead");
        }
    }

    /// <summary>
    /// Swaps buffers for the rendering thread.
    /// </summary>
    public async Task SwapBuffers()
    {
        if (_bufferQueue == null || _glContext == null)
            return;

        // ASSUME that the context is current
        using (_glContext.MakeCurrent())
        {
            var gl = GL.GetApi(_glContext.GlInterface.GetProcAddress);

            gl.Flush();
        }

        await _bufferQueue.SwapBuffers(WindowSize);
        using (_glContext.MakeCurrent())
        {
            var gl = GL.GetApi(_glContext.GlInterface.GetProcAddress);

            var curr = _bufferQueue.CurrentBuffer;
            InitGLBuffers(gl, curr); 
        }
    }

    public uint RenderFBO => _renderFbo;

    public GlVersion? ContextVersion => _glContext?.Version;
}