using System;
using Mupen64PlusRR.Models.Emulation;
using Silk.NET.SDL;

namespace Mupen64PlusRR.Controls.Helpers;

public static class SDLHelpers
{
    public static GLattr ToSdlAttr(Mupen64Plus.GLAttribute attr)
    {
        return attr switch
        {
            Mupen64Plus.GLAttribute.DoubleBuffer => GLattr.Doublebuffer,
            Mupen64Plus.GLAttribute.BufferSize => GLattr.BufferSize,
            Mupen64Plus.GLAttribute.DepthSize => GLattr.DepthSize,
            Mupen64Plus.GLAttribute.RedSize => GLattr.RedSize,
            Mupen64Plus.GLAttribute.GreenSize => GLattr.GreenSize,
            Mupen64Plus.GLAttribute.BlueSize => GLattr.BlueSize,
            Mupen64Plus.GLAttribute.AlphaSize => GLattr.AlphaSize,
            Mupen64Plus.GLAttribute.MultisampleBuffers => GLattr.Multisamplebuffers,
            Mupen64Plus.GLAttribute.MultisampleSamples => GLattr.Multisamplesamples,
            Mupen64Plus.GLAttribute.ContextMajorVersion => GLattr.ContextMajorVersion,
            Mupen64Plus.GLAttribute.ContextMinorVersion => GLattr.ContextMinorVersion,
            _ => (GLattr) (-1)
        };
    }

    public static unsafe int GetMupenGLAttribute(this Sdl sdl, Mupen64Plus.GLAttribute attr)
    {
        switch (attr)
        {
            case Mupen64Plus.GLAttribute.SwapControl:
                return sdl.GLGetSwapInterval();
            case Mupen64Plus.GLAttribute.ContextProfileMask:
            {
                GLprofile profile = GLprofile.None;
                sdl.GLGetAttribute(GLattr.ContextProfileMask, (int*) &profile);
                return profile switch
                {
                    GLprofile.Core => (int) Mupen64Plus.GLContextType.Core,
                    GLprofile.Compatibility => (int) Mupen64Plus.GLContextType.Compatibilty,
                    GLprofile.ES => (int) Mupen64Plus.GLContextType.ES,
                    _ => 0
                };
            }
            default:
            {
                int value = 0;
                var sdlAttr = ToSdlAttr(attr);
                
                if (sdlAttr == (GLattr) (-1))
                    return 0;

                sdl.GLGetAttribute(sdlAttr, ref value);
                return value;
            }
        }
    }

    public static void SetMupenGLAttribute(this Sdl sdl, Mupen64Plus.GLAttribute attr, int value)
    {
        switch (attr)
        {
            case Mupen64Plus.GLAttribute.SwapControl:
                sdl.GLSetSwapInterval(value);
                return;
            case Mupen64Plus.GLAttribute.ContextProfileMask:
            {
                GLprofile profile = (Mupen64Plus.GLContextType) value switch
                {
                    Mupen64Plus.GLContextType.Core => GLprofile.Core,
                    Mupen64Plus.GLContextType.Compatibilty => GLprofile.Compatibility,
                    Mupen64Plus.GLContextType.ES => GLprofile.ES,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
                sdl.GLSetAttribute(GLattr.ContextProfileMask, (int) profile);
                return;
            }
            default:
            {
                var sdlAttr = ToSdlAttr(attr);
                if (sdlAttr == (GLattr) (-1))
                    return;

                sdl.GLSetAttribute(sdlAttr, value);
                return;
            }
        }
    }
}