using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using System.CodeDom;
using OpenTK.Graphics.Wgl;

namespace M64RPFW.Wpf.Helpers
{
    internal class GLFWHelpers
    {
        public static void InterpretAttributes(IDictionary<GLAttribute, int> attrs)
        {
            if (attrs.TryGetValue(GLAttribute.DoubleBuffer, out int value))
                GLFW.WindowHint(WindowHintBool.DoubleBuffer, value != 0);
            if (attrs.TryGetValue(GLAttribute.BufferSize, out value))
            {
                switch (value)
                {
                    case 32:
                    {
                        GLFW.WindowHint(WindowHintInt.RedBits, 8);
                        GLFW.WindowHint(WindowHintInt.GreenBits, 8);
                        GLFW.WindowHint(WindowHintInt.BlueBits, 8);
                        GLFW.WindowHint(WindowHintInt.AlphaBits, 8);
                    } break;
                    case 24:
                    {
                        GLFW.WindowHint(WindowHintInt.RedBits, 8);
                        GLFW.WindowHint(WindowHintInt.GreenBits, 8);
                        GLFW.WindowHint(WindowHintInt.BlueBits, 8);
                        GLFW.WindowHint(WindowHintInt.AlphaBits, 0);
                    } break;
                }
            }
            if (attrs.TryGetValue(GLAttribute.DepthSize, out value))
                GLFW.WindowHint(WindowHintInt.DepthBits, value);
            if (attrs.TryGetValue(GLAttribute.RedSize, out value))
                GLFW.WindowHint(WindowHintInt.RedBits, value);
            if (attrs.TryGetValue(GLAttribute.GreenSize, out value))
                GLFW.WindowHint(WindowHintInt.GreenBits, value);
            if (attrs.TryGetValue(GLAttribute.BlueSize, out value))
                GLFW.WindowHint(WindowHintInt.BlueBits, value);
            if (attrs.TryGetValue(GLAttribute.AlphaSize, out value))
                GLFW.WindowHint(WindowHintInt.AlphaBits, value);
            if (attrs.TryGetValue(GLAttribute.MultisampleSamples, out value))
                GLFW.WindowHint(WindowHintInt.Samples, value);
            if (attrs.TryGetValue(GLAttribute.ContextMajorVersion, out value))
                GLFW.WindowHint(WindowHintInt.ContextVersionMajor, value);
            if (attrs.TryGetValue(GLAttribute.ContextMinorVersion, out value))
                GLFW.WindowHint(WindowHintInt.ContextVersionMinor, value);
            if (attrs.TryGetValue(GLAttribute.ContextProfileMask, out value))
            {
                GLContextType type = (GLContextType) value;
                if (type == GLContextType.ES)
                {
                    GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlEsApi);
                    return;
                }

                GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);

                OpenGlProfile profile = (type == GLContextType.Core) ? OpenGlProfile.Core : OpenGlProfile.Compat;
                GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, profile);
            }
        } 

        public static unsafe int QueryAttribute(GLAttribute attr, Window* window)
        {
            GL.LoadBindings(new GLFWBindingsContext());
            switch (attr)
            {
                case GLAttribute.DoubleBuffer:
                {
                    GLFW.MakeContextCurrent(window);
                    bool result = GL.GetBoolean(GetPName.Doublebuffer);
                    return (result) ? 1 : 0;
                }
                case GLAttribute.BufferSize:
                {
                    GLFW.MakeContextCurrent(window);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentRedSize,
                        out int redSize);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentGreenSize,
                        out int greenSize);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentBlueSize,
                        out int blueSize);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentAlphaSize,
                        out int alphaSize);
                    return redSize + greenSize + blueSize + alphaSize;
                }
                case GLAttribute.DepthSize:
                {
                    GLFW.MakeContextCurrent(window);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentDepthSize,
                        out int depthSize);
                    return depthSize;
                }
                case GLAttribute.RedSize:
                {
                    GLFW.MakeContextCurrent(window);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentRedSize,
                        out int redSize);
                    return redSize;
                }
                case GLAttribute.GreenSize:
                {
                    GLFW.MakeContextCurrent(window);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentGreenSize,
                        out int greenSize);
                    return greenSize;
                }
                case GLAttribute.BlueSize:
                {
                    GLFW.MakeContextCurrent(window);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentBlueSize,
                        out int blueSize);
                    return blueSize;
                }
                case GLAttribute.AlphaSize:
                {
                    GLFW.MakeContextCurrent(window);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.BackLeft,
                        FramebufferParameterName.FramebufferAttachmentAlphaSize,
                        out int alphaSize);
                    return alphaSize;
                }
                case GLAttribute.SwapControl:
                    throw new NotImplementedException("Can't query swap interval");
                case GLAttribute.MultisampleBuffers:
                {
                    GLFW.MakeContextCurrent(window);
                    return GL.GetInteger(GetPName.SampleBuffers);
                }
                case GLAttribute.MultisampleSamples:
                {
                    GLFW.MakeContextCurrent(window);
                    return GL.GetInteger(GetPName.Samples);
                }
                case GLAttribute.ContextMajorVersion:
                {
                    return GLFW.GetWindowAttrib(
                        window, WindowAttributeGetInt.ContextVersionMajor);
                }
                case GLAttribute.ContextMinorVersion:
                {
                    return GLFW.GetWindowAttrib(
                        window, WindowAttributeGetInt.ContextVersionMinor);
                }
                case GLAttribute.ContextProfileMask:
                {
                    ClientApi api = GLFW.GetWindowAttrib(
                        window, WindowAttributeGetClientApi.ClientApi);
                    if (api == ClientApi.OpenGlEsApi)
                        return (int) GLContextType.ES;

                    OpenGlProfile profile = GLFW.GetWindowAttrib(
                        window, WindowAttributeGetOpenGlProfile.OpenGlProfile);

                    return (profile == OpenGlProfile.Core) ?
                        (int) GLContextType.Core : (int) GLContextType.Compatibilty;
                }
            }
            throw new ApplicationException("QueryAttribute reached end of switch");
        }
    }
}
