using System;
using M64RPFW.Models.Types;
using Silk.NET.SDL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class SDLHelpers
{
    public static GLattr ToSdlAttr(Mupen64PlusTypes.GLAttribute attr)
    {
        return attr switch
        {
            Mupen64PlusTypes.GLAttribute.DoubleBuffer => GLattr.Doublebuffer,
            Mupen64PlusTypes.GLAttribute.BufferSize => GLattr.BufferSize,
            Mupen64PlusTypes.GLAttribute.DepthSize => GLattr.DepthSize,
            Mupen64PlusTypes.GLAttribute.RedSize => GLattr.RedSize,
            Mupen64PlusTypes.GLAttribute.GreenSize => GLattr.GreenSize,
            Mupen64PlusTypes.GLAttribute.BlueSize => GLattr.BlueSize,
            Mupen64PlusTypes.GLAttribute.AlphaSize => GLattr.AlphaSize,
            Mupen64PlusTypes.GLAttribute.MultisampleBuffers => GLattr.Multisamplebuffers,
            Mupen64PlusTypes.GLAttribute.MultisampleSamples => GLattr.Multisamplesamples,
            Mupen64PlusTypes.GLAttribute.ContextMajorVersion => GLattr.ContextMajorVersion,
            Mupen64PlusTypes.GLAttribute.ContextMinorVersion => GLattr.ContextMinorVersion,
            _ => (GLattr) (-1)
        };
    }

    public static unsafe int GetMupenGLAttribute(this Sdl sdl, Mupen64PlusTypes.GLAttribute attr)
    {
        switch (attr)
        {
            case Mupen64PlusTypes.GLAttribute.SwapControl:
                return sdl.GLGetSwapInterval();
            case Mupen64PlusTypes.GLAttribute.ContextProfileMask:
            {
                GLprofile profile = GLprofile.None;
                sdl.GLGetAttribute(GLattr.ContextProfileMask, (int*) &profile);
                return profile switch
                {
                    GLprofile.Core => (int) Mupen64PlusTypes.GLContextType.Core,
                    GLprofile.Compatibility => (int) Mupen64PlusTypes.GLContextType.Compatibilty,
                    GLprofile.ES => (int) Mupen64PlusTypes.GLContextType.ES,
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

    public static void SetMupenGLAttribute(this Sdl sdl, Mupen64PlusTypes.GLAttribute attr, int value)
    {
        switch (attr)
        {
            case Mupen64PlusTypes.GLAttribute.SwapControl:
                sdl.GLSetSwapInterval(value);
                return;
            case Mupen64PlusTypes.GLAttribute.ContextProfileMask:
            {
                GLprofile profile = (Mupen64PlusTypes.GLContextType) value switch
                {
                    Mupen64PlusTypes.GLContextType.Core => GLprofile.Core,
                    Mupen64PlusTypes.GLContextType.Compatibilty => GLprofile.Compatibility,
                    Mupen64PlusTypes.GLContextType.ES => GLprofile.ES,
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