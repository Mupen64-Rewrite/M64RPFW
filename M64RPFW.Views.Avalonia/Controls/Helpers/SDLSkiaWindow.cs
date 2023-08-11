using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkiaSharp;
using static M64RPFW.Views.Avalonia.Controls.Helpers.SDLHelpers;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

internal sealed unsafe class SDLSkiaWindow : IDisposable
{
    private static readonly float[] FullscreenQuadVertices = {
        -1.0f, +1.0f,
        +1.0f, +1.0f,
        -1.0f, -1.0f,
        +1.0f, -1.0f,
    };
    
    private Window* _win;
    private void* _ctx;
    private GL _gl;

    private uint _texture;
    private uint _fbo;
    private uint _dsRbo;
    
    private uint _vertexBuffer;
    private uint _blitQuadProgram;
    private int _texUniformID;

    private GRContext _grContext;
    private SKSurface? _surface;

    public SDLSkiaWindow(Window* srcWin, void* srcCtx)
    {
        _win = sdl.CreateWindow("", 0, 0, 1, 1, WindowFlags.Opengl | WindowFlags.Hidden);
        if (_win == null)
            throw new SDLException();
        using (sdl.GLMakeCurrentTemp(srcWin, srcCtx))
        {
            try
            {
                sdl.GLResetAttributes();
                sdl.GLSetAttribute(GLattr.ContextMajorVersion, 4);
                sdl.GLSetAttribute(GLattr.ContextMinorVersion, 0);
                sdl.GLSetAttribute(GLattr.ContextProfileMask, (int) GLprofile.Compatibility);
                sdl.GLSetAttribute(GLattr.Doublebuffer, 1);
                sdl.GLSetAttribute(GLattr.ShareWithCurrentContext, 1);
                sdl.GLSetAttribute(GLattr.StencilSize, 8);
                sdl.GLSetAttribute(GLattr.ContextFlags, (int) GLcontextFlag.DebugFlag);

                _ctx = sdl.GLCreateContext(_win);
                if (_ctx == null)
                    throw new SDLException();
            }
            finally
            {
                sdl.GLResetAttributes();
            }
        }

        using (sdl.GLMakeCurrentTemp(_win, _ctx))
        {
            _gl = sdl.GetGLBinding();
            _gl.AttachDebugLogger();
            _texture = 0;
            _fbo = 0;
            _dsRbo = 0;

            _vertexBuffer = 0;
            _blitQuadProgram = 0;

            _grContext = GRContext.CreateGl();
            if (_grContext == null)
                throw new SystemException("GRContext.CreateGl() failed");
        }
    }

    private static uint LoadBufferObject<T>(GL gl, BufferTargetARB target, ReadOnlySpan<T> data) where T : unmanaged
    {
        uint buffer = gl.GenBuffer();
        gl.BindBuffer(target, buffer);
        gl.BufferData(target, data, BufferUsageARB.StaticDraw);
        return buffer;
    }
    
    private static uint LinkBlitQuadShader(GL gl, out int texLocation)
    {
        uint vertShader = gl.CreateShader(ShaderType.VertexShader);
        uint fragShader = gl.CreateShader(GLEnum.FragmentShader);
        texLocation = -1;
        
        try
        {
            gl.ShaderSourceFromResources(vertShader, "/Resources/Shaders/blit_quad.vert.glsl");
            gl.CompileShaderChecked(vertShader);

            gl.ShaderSourceFromResources(fragShader, "/Resources/Shaders/blit_quad.frag.glsl");
            gl.CompileShaderChecked(fragShader);

            uint prog = gl.CreateProgram();
            gl.AttachShader(prog, vertShader);
            gl.AttachShader(prog, fragShader);
        
            gl.LinkProgramChecked(prog);
        
            gl.DetachShader(prog, vertShader);
            gl.DetachShader(prog, fragShader);

            texLocation = gl.GetUniformLocation(prog, "tex");

            return prog;
        }
        finally
        {
            gl.DeleteShader(vertShader);
            gl.DeleteShader(fragShader);
        }
    }

    public void MakeCurrent()
    {
        if (sdl.GLMakeCurrent(_win, _ctx) < 0)
            throw new SDLException();
    }

    public IDisposable MakeCurrentTemp()
    {
        return sdl.GLMakeCurrentTemp(_win, _ctx);
    }

    public bool HasSurface => _surface != null;

    public void InitSurface(PixelSize size)
    {
        Debug.Assert(sdl.GLGetCurrentContext() == _ctx, "Context not current");
        if (size.Width == 0 || size.Height == 0)
            return;

        if (_fbo == 0)
        {
            _fbo = _gl.GenFramebuffer();
        }
        if (_dsRbo != 0)
        {
            _gl.DeleteRenderbuffer(_dsRbo);
        }
        if (_texture != 0)
        {
            _gl.DeleteTexture(_texture);
        }
        

        _texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) size.Width, (uint) size.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Nearest);

        _dsRbo = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _dsRbo);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (uint) size.Width, (uint) size.Height);
        
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _dsRbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texture, 0);
        
        #if DEBUG
        {
            var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            Debug.Assert(status == GLEnum.FramebufferComplete, "status == GLEnum.FramebufferComplete", "status code: GLEnum.{0} (0x{1:X4})", status, (uint) status);
        }
        #endif
        
        _gl.DrawBuffer(DrawBufferMode.ColorAttachment0);
        
        var desc = new GRBackendRenderTarget(size.Width, size.Height, 0, 8, new GRGlFramebufferInfo(_fbo, (uint) InternalFormat.Rgba));
        _surface = SKSurface.Create(_grContext, desc, GRSurfaceOrigin.BottomLeft, SKColorType.RgbaF32);
        
        _grContext.ResetContext();
    }

    public void DoRender(Action<SKCanvas>? render)
    {
        Debug.Assert(sdl.GLGetCurrentContext() == _ctx, "Context not current");
        Debug.Assert(_surface != null, "_surface not initialized");
        
        
        _surface.Canvas.Clear();
        if (render == null)
            return;

        render(_surface.Canvas);
        
        _surface.Flush(true);
    }

    public void BlitQuad(GL gl, PixelSize viewport)
    {
        uint prevProgram = (uint) gl.GetInteger(GetPName.CurrentProgram);
        uint prevVAO = (uint) gl.GetInteger(GetPName.VertexArrayBinding);
        uint prevBuffer = (uint) gl.GetInteger(GetPName.ArrayBufferBinding);
        uint prevEBO = (uint) gl.GetInteger(GetPName.ElementArrayBufferBinding);
        var prevActiveTexture = (GLEnum) gl.GetInteger(GetPName.ActiveTexture);
        
        bool prevDepthTest = gl.IsEnabled(EnableCap.DepthTest);
        bool prevScissorTest = gl.IsEnabled(EnableCap.ScissorTest);
        bool prevBlend = gl.IsEnabled(EnableCap.Blend);

        var prevSrcColorBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendSrcRgb);
        var prevDstColorBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendDstRgb);
        var prevSrcAlphaBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendSrcAlpha);
        var prevDstAlphaBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendDstAlpha);
        var prevColorBlendEqn = (BlendEquationModeEXT) gl.GetInteger(GetPName.BlendEquationRgb);
        var prevAlphaBlendEqn = (BlendEquationModeEXT) gl.GetInteger(GetPName.BlendEquationAlpha);

        uint[] prevViewport = new uint[4];
        fixed (uint* pPrevViewport = prevViewport)
            gl.GetInteger(GetPName.Viewport, (int*) pPrevViewport);

        uint vao = 0;
        try
        {
            #if false
            gl.ClearColor(1.0f, 0.5f, 0.0f, 1.0f);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            #else
            if (_blitQuadProgram == 0)
                _blitQuadProgram = LinkBlitQuadShader(_gl, out _texUniformID);
            if (_vertexBuffer == 0)
                _vertexBuffer = LoadBufferObject<float>(_gl, BufferTargetARB.ArrayBuffer, FullscreenQuadVertices);
            
            // We want to render over everything
            gl.Disable(EnableCap.DepthTest);
            gl.Disable(EnableCap.ScissorTest);
            gl.Disable(EnableCap.CullFace);
            // If Skia uses partial transparency, it needs to work
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
            // Set the fullscreen viewport
            gl.Viewport(0, 0, (uint) viewport.Width, (uint) viewport.Height);
            
            // Switch to our own shader and attach the texture
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, _texture);
            gl.UseProgram(_blitQuadProgram);
            gl.Uniform1(_texUniformID, 0);

            // Setup the vertex buffers to draw our fullscreen texture
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
            
            // Draw the quad
            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, (uint) (FullscreenQuadVertices.Length / 2));
            #endif
        }
        finally
        {
            // delete temporary stuff
            if (vao != 0)
                gl.DeleteVertexArray(vao);
            
            // Reset every piece of state we set earlier (this might be slow)
            gl.UseProgram(prevProgram);
            gl.BindVertexArray(prevVAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, prevBuffer);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, prevEBO);
            gl.ActiveTexture(prevActiveTexture);
            
            gl.SetCap(EnableCap.DepthTest, prevDepthTest);
            gl.SetCap(EnableCap.ScissorTest, prevScissorTest);
            gl.SetCap(EnableCap.Blend, prevBlend);

            gl.BlendFuncSeparate(prevSrcColorBlend, prevDstColorBlend, prevSrcAlphaBlend, prevDstAlphaBlend);
            gl.BlendEquationSeparate(prevColorBlendEqn, prevAlphaBlendEqn);
            
            gl.Viewport((int) prevViewport[0], (int) prevViewport[1], prevViewport[2], prevViewport[3]);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        using (sdl.GLMakeCurrentTemp(_win, _ctx))
        {
            _gl.DeleteTexture(_texture);
            _gl.DeleteBuffers(new[] {_vertexBuffer});
            _gl.DeleteProgram(_blitQuadProgram);
        }
        sdl.GLMakeCurrent(_win, null);
        sdl.GLDeleteContext(_ctx);
        sdl.DestroyWindow(_win);
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _grContext.Dispose();

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SDLSkiaWindow()
    {
        ReleaseUnmanagedResources();
    }
}