using System;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public class GLException : SystemException
{
    public GLException()
    {
    }

    public GLException(string? message) : base(message)
    {
    }

    public GLException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class ShaderCompileException : GLException
{
    public ShaderCompileException(string? message) : base(message)
    {
    }

    public ShaderCompileException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class ProgramLinkException : GLException
{
    public ProgramLinkException(string? message) : base(message)
    {
    }

    public ProgramLinkException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}