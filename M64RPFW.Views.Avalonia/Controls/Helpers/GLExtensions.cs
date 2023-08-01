using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class GLExtensions
{
    public static void Viewport(this GL gl, PixelRect rect)
    {
        gl.Viewport(rect.X, rect.Y, (uint) rect.Width, (uint) rect.Height);
    }

    public static void Viewport(this GL gl, PixelPoint topLeft, PixelSize size)
    {
        gl.Viewport(topLeft.X, topLeft.Y, (uint) size.Width, (uint) size.Height);
    }

    /// <summary>
    /// Loads a GLSL shader from an assembly's resources.
    /// </summary>
    /// <param name="gl">The OpenGL API to use</param>
    /// <param name="shader">ID of the shader object to load into</param>
    /// <param name="resPath">The path to the shader's resource file.</param>
    /// <param name="src">The assembly to load from. If not set, uses the M64RPFW assembly.</param>
    /// <exception cref="ArgumentException"></exception>
    public static unsafe void ShaderSourceFromResources(this GL gl, uint shader, string resPath, Assembly? src = null)
    {
        src ??= typeof(GLExtensions).Assembly;
        using var resStream = src.GetManifestResourceStream(resPath) ?? 
                                 throw new ArgumentException($"Resource does not exist ({resPath})");
        using var bufStream = new MemoryStream();
        
        resStream.CopyTo(bufStream);
            
        var buf = bufStream.GetBuffer();
        var len = buf.Length;
        fixed (byte* pBuf = buf)
        {
            gl.ShaderSource(shader, in pBuf, new ReadOnlySpan<int>(in len));
        }
    }
    
    /// <summary>
    /// Compiles a GLSL shader, throwing a <see cref="ShaderCompileException"/> if compilation failed.
    /// </summary>
    /// <param name="gl">The OpenGL API to use</param>
    /// <param name="shader">ID of the shader object to compile</param>
    /// <exception cref="ShaderCompileException">If compilation fails.</exception>
    public static void CompileShaderChecked(this GL gl, uint shader)
    {
        gl.CompileShader(shader);
        if (gl.GetShader(shader, ShaderParameterName.CompileStatus) == 0)
        {
            throw new ShaderCompileException(gl.GetShaderInfoLog(shader));
        }
    }

    /// <summary>
    /// Links a shader program, throwing a <see cref="ProgramLinkException"/> if linking fails.
    /// </summary>
    /// <param name="gl">The OpenGL API to use</param>
    /// <param name="program">ID of the program object to link</param>
    /// <exception cref="ProgramLinkException"></exception>
    public static void LinkProgramChecked(this GL gl, uint program)
    {
        gl.LinkProgram(program);
        if (gl.GetProgram(program, ProgramPropertyARB.LinkStatus) == 0)
        {
            throw new ProgramLinkException(gl.GetProgramInfoLog(program));
        }
    }

    public static void SetCap(this GL gl, EnableCap cap, bool value)
    {
        if (value)
            gl.Enable(cap);
        else
            gl.Disable(cap);
    }
}