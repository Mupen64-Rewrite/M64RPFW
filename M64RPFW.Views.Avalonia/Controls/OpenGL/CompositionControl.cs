using System;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public abstract class CompositionControl : Control
{
    private Task? _initTask;
    private Compositor? _compositor;
    private CompositionSurfaceVisual? _visual;

    protected CompositionDrawingSurface? Surface { get; private set; }
    protected ICompositionGpuInterop? Interop { get; private set; }

    public PixelSize WindowSize { get; set; }

    protected abstract Task InitGpuResources(Compositor compositor, CompositionDrawingSurface surface,
        ICompositionGpuInterop interop);

    protected abstract Task FreeGpuResources();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _initTask = Initialize();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Cleanup();
    }

    private async Task Initialize()
    {
        _compositor = ElementComposition.GetElementVisual(this)!.Compositor;
        Surface = _compositor.CreateDrawingSurface();

        Interop = await _compositor.TryGetCompositionGpuInterop();
        if (Interop == null)
            throw new PlatformNotSupportedException("GPU interop not supported");
        await InitGpuResources(_compositor, Surface, Interop);

        _visual = _compositor.CreateSurfaceVisual();
        _visual.Size = new Vector2(WindowSize.Width, WindowSize.Height);
        _visual.Surface = Surface;
        ElementComposition.SetElementChildVisual(this, _visual);
    }

    private async void Cleanup()
    {
        if (_initTask is { Status: TaskStatus.RanToCompletion })
        {
            await FreeGpuResources();
        }

        ElementComposition.SetElementChildVisual(this, null);
        _visual = null;
        _initTask = null;
    }
}