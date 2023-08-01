using System;
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
        +1.0f, +1.0f,
        -1.0f, +1.0f,
        -1.0f, -1.0f,
        +1.0f, -1.0f
    };
    private static readonly float[] FullscreenQuadTexCoords =
    {
        1.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 0.0f,
        1.0f, 0.0f
    };
    private static readonly uint[] FullscreenQuadElements =
    {
        0, 1, 2,
        0, 2, 3
    };
    
    private Window* _win;
    private void* _ctx;
    private GL _gl;

    private uint _texture;
    private uint _vertexBuffer;
    private uint _texCoordBuffer;
    private uint _elementBuffer;
    private uint _blitQuadProgram;

    private GRContext _grContext;
    private SKSurface? _surface;

    public SDLSkiaWindow(Window* srcWin, void* srcCtx)
    {
        _win = sdl.CreateWindow("", 0, 0, 1, 1, WindowFlags.Opengl | WindowFlags.Hidden);
        if (_win == null)
            throw new SDLException();
        using (sdl.GLMakeCurrentTemp(srcWin, srcCtx))
        {
            sdl.GLResetAttributes();
            sdl.GLSetAttribute(GLattr.ContextMajorVersion, 3);
            sdl.GLSetAttribute(GLattr.ContextMinorVersion, 3);
            sdl.GLSetAttribute(GLattr.ContextProfileMask, (int) GLprofile.Core);
            sdl.GLSetAttribute(GLattr.Doublebuffer, 1);
            sdl.GLSetAttribute(GLattr.ShareWithCurrentContext, 1);
            sdl.GLSetAttribute(GLattr.StencilSize, 8);

            _ctx = sdl.GLCreateContext(_win);
            if (_ctx == null)
                throw new SDLException();
        }

        using (sdl.GLMakeCurrentTemp(_win, _ctx))
        {
            _gl = sdl.GetGLBinding();
            _texture = 0;

            _blitQuadProgram = LinkBlitQuadShader(_gl);

            _vertexBuffer = LoadBufferObject<float>(_gl, BufferTargetARB.ArrayBuffer, FullscreenQuadVertices);
            _texCoordBuffer = LoadBufferObject<float>(_gl, BufferTargetARB.ArrayBuffer, FullscreenQuadTexCoords);
            _elementBuffer = LoadBufferObject<uint>(_gl, BufferTargetARB.ElementArrayBuffer, FullscreenQuadElements);

            _grContext = GRContext.CreateGl();
            if (_grContext == null)
                throw new SystemException("GRContext.CreateGl() failed");
        }
    }

    private static uint LoadBufferObject<T>(GL gl, BufferTargetARB target, ReadOnlySpan<T> data) where T : unmanaged
    {
        uint buffer = gl.GenBuffer();
        gl.BindBuffer(target, buffer);
        gl.BufferData(GLEnum.ArrayBuffer, data, BufferUsageARB.StaticDraw);
        return buffer;
    }
    
    private static uint LinkBlitQuadShader(GL gl)
    {
        uint vertShader = gl.CreateShader(ShaderType.VertexShader);
        uint fragShader = gl.CreateShader(GLEnum.FragmentShader);

        try
        {
            gl.ShaderSourceFromResources(vertShader, "/Assets/Shaders/blit_quad.vert.glsl");
            gl.CompileShaderChecked(vertShader);

            gl.ShaderSourceFromResources(fragShader, "/Assets/Shaders/blit_quad.frag.glsl");
            gl.CompileShaderChecked(fragShader);

            uint prog = gl.CreateProgram();
            gl.AttachShader(prog, vertShader);
            gl.AttachShader(prog, fragShader);
        
            gl.LinkProgramChecked(prog);
        
            gl.DetachShader(prog, vertShader);
            gl.DetachShader(prog, fragShader);

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

    public void InitSurface(PixelSize size)
    {
        CheckContext();
        if (_texture != 0)
        {
            _gl.DeleteTexture(_texture);
        }
        
        _texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) size.Width, (uint) size.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);

        _surface = SKSurface.Create(_grContext, 
            new GRBackendTexture(size.Width, size.Height, false, new GRGlTextureInfo((uint) TextureTarget.Texture2D, _texture)), SKColorType.Rgba8888);
    }

    public void BlitTexture(GL gl)
    {
        uint prevProgram = (uint) gl.GetInteger(GetPName.CurrentProgram);
        uint prevVAO = (uint) gl.GetInteger(GetPName.VertexArrayBinding);
        uint prevBuffer = (uint) gl.GetInteger(GetPName.ArrayBufferBinding);
        uint prevEBO = (uint) gl.GetInteger(GetPName.ElementArrayBufferBinding);
        var prevActiveTexture = (GLEnum) gl.GetInteger(GetPName.ActiveTexture);
        
        bool prevDepthTest = gl.IsEnabled(EnableCap.DepthTest);
        bool prevBlend = gl.IsEnabled(EnableCap.Blend);

        var prevSrcColorBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendSrcRgb);
        var prevDstColorBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendDstRgb);
        var prevSrcAlphaBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendSrcAlpha);
        var prevDstAlphaBlend = (BlendingFactor) gl.GetInteger(GetPName.BlendDstAlpha);
        var prevColorBlendEqn = (BlendEquationModeEXT) gl.GetInteger(GetPName.BlendEquationRgb);
        var prevAlphaBlendEqn = (BlendEquationModeEXT) gl.GetInteger(GetPName.BlendEquationAlpha);

        uint vao = 0;
        try
        {
            // We want to render over everything
            gl.Disable(EnableCap.DepthTest);
            // If Skia uses partial transparency, it needs to work
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
            
            // Switch to our own shader and attach the texture
            gl.UseProgram(_blitQuadProgram);
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, _texture);
            gl.Uniform1(0, 0);
            
            // Setup the vertex buffers to draw our fullscreen texture
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            
            gl.EnableVertexAttribArray(0);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
            
            gl.EnableVertexAttribArray(1);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _texCoordBuffer);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
            
            // Draw the quad
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _elementBuffer);
            gl.DrawElements(PrimitiveType.Triangles, (uint) FullscreenQuadElements.Length, DrawElementsType.UnsignedInt, null);
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
            gl.SetCap(EnableCap.Blend, prevBlend);

            gl.BlendFuncSeparate(prevSrcColorBlend, prevDstColorBlend, prevSrcAlphaBlend, prevDstAlphaBlend);
            gl.BlendEquationSeparate(prevColorBlendEqn, prevAlphaBlendEqn);
        }
    }

    private void CheckContext()
    {
        #if DEBUG
        if (sdl.GLGetCurrentContext() != _ctx)
            throw new InvalidOperationException("The context must be current first");
        #endif
    }

    private void ReleaseUnmanagedResources()
    {
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