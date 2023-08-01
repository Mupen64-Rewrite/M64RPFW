using System;
using System.Reflection;
using Avalonia;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkiaSharp;
using static M64RPFW.Views.Avalonia.Controls.Helpers.SDLHelpers;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

internal sealed unsafe class SDLSkiaWindow : IDisposable
{
    Window* _win;
    void* _ctx;
    GL _gl;

    uint _blitQuadProgram;
    uint _texture;

    GRContext _grContext;
    SKSurface? _surface;

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
            _blitQuadProgram = LinkBlitQuadShader(_gl);

            _grContext = GRContext.CreateGl();
            if (_grContext == null)
                throw new SystemException("GRContext.CreateGl() failed");
        }
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
    }

    public void BlitTexture(GL gl)
    {
        CheckContext();
        
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