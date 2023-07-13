using System;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using M64RPFW.Views.Avalonia.Controls.Helpers;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public abstract class CompositionControl : Control
{
    private CompositionSurfaceVisual? _visual;
    private PixelSize _windowSize;

    public static readonly DirectProperty<CompositionControl, PixelSize> WindowSizeProperty =
        AvaloniaProperty.RegisterDirect<CompositionControl, PixelSize>(nameof(WindowSize), c => c._windowSize,
            (c, value) => c._windowSize = value);

    static CompositionControl()
    {
        WindowSizeProperty.Changed.Subscribe(e =>
        {
            var @this = (CompositionControl) e.Sender;
            var size = e.NewValue.Value;
            if (@this._visual != null)
                @this._visual.Size = new Vector2(size.Width, size.Height);
        });
    }

    protected Task? InitTask { get; private set; }
    protected Compositor? Compositor { get; private set; }
    protected CompositionDrawingSurface? Surface { get; private set; }
    protected ICompositionGpuInterop? Interop { get; private set; }

    public PixelSize WindowSize
    {
        get => _windowSize;
        set => SetAndRaise(WindowSizeProperty, ref _windowSize, value);
    }

    /// <summary>
    /// Initializes API-specific objects for texture sharing.
    /// </summary>
    /// <param name="compositor">The <see cref="Compositor"/> associated with this control</param>
    /// <param name="surface">The <see cref="CompositionDrawingSurface"/> to target</param>
    /// <param name="interop">An <see cref="ICompositionGpuInterop"/> allowing interop with GPU frameworks</param>
    /// <returns>A task that completes when initialization completes.</returns>
    protected abstract Task InitGpuResources(Compositor compositor, CompositionDrawingSurface surface,
        ICompositionGpuInterop interop);

    /// <summary>
    /// Frees any resources that were allocated through <see cref="InitGpuResources"/>.
    /// </summary>
    /// <returns></returns>
    protected abstract Task FreeGpuResources();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InitTask = Initialize();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Cleanup();
    }

    private async Task Initialize()
    {
        Compositor = ElementComposition.GetElementVisual(this)!.Compositor;
        Surface = Compositor.CreateDrawingSurface();

        Interop = await Compositor.TryGetCompositionGpuInterop();
        if (Interop == null)
            throw new PlatformNotSupportedException("GPU interop not supported");
        await InitGpuResources(Compositor, Surface, Interop);

        _visual = Compositor.CreateSurfaceVisual();
        _visual.Size = new Vector2(WindowSize.Width, WindowSize.Height);
        _visual.Surface = Surface;
        ElementComposition.SetElementChildVisual(this, _visual);
    }

    private async void Cleanup()
    {
        if (InitTask is { Status: TaskStatus.RanToCompletion })
        {
            await FreeGpuResources();
        }

        ElementComposition.SetElementChildVisual(this, null);
        _visual = null;
        InitTask = null;
    }
}