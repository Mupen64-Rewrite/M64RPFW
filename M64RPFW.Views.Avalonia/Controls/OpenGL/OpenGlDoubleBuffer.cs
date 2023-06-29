using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using M64RPFW.Views.Avalonia.Controls.Helpers;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public class OpenGlDoubleBuffer : DoubleBuffer<ICompositionImportableOpenGlSharedTexture>
{
    private IGlContext _glContext;
    private IOpenGlTextureSharingRenderInterfaceContextFeature _sharingFeature;

    public OpenGlDoubleBuffer(Compositor compositor, ICompositionGpuInterop interop, CompositionDrawingSurface target,
        IGlContext glContext, IOpenGlTextureSharingRenderInterfaceContextFeature sharingFeature,
        bool vSync = false) : base(sharingFeature.CreateSharedTextureForComposition(glContext, new PixelSize(640, 480)), compositor, interop, target, vSync)
    {
        _glContext = glContext;
        _sharingFeature = sharingFeature;

    }

    protected override void EnsureBufferInitialized([NotNull] ref ICompositionImportableOpenGlSharedTexture? buffer, PixelSize size)
    {
        if (buffer != null && buffer.Size != size)
        {
            buffer.Dispose();
            buffer = null;
        }
        buffer ??= _sharingFeature.CreateSharedTextureForComposition(_glContext, size);
    }
}