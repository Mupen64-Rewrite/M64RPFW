using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public class GLQueuableImage : IQueuableImage
{
    private ICompositionImportableOpenGlSharedTexture _texture;

    public GLQueuableImage(PixelSize size, IOpenGlTextureSharingRenderInterfaceContextFeature glSharing, IGlContext context)
    {
        _texture = glSharing.CreateSharedTextureForComposition(context, size);
    }

    public uint TextureObject => (uint) _texture.TextureId;
    
    public ValueTask DisposeAsync()
    {
        _texture.Dispose();
        return ValueTask.CompletedTask;
    }

    public PixelSize Size => _texture.Size;

    public ICompositionImportedGpuImage Import(ICompositionGpuInterop interop)
    {
        return interop.ImportImage(_texture);
    }
}