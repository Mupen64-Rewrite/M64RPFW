using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace M64RPFW.Views.Avalonia.Controls;

public class SkiaRenderEventArgs
{
    public SkiaCanvas Sender { get; init; }
    public SKCanvas Canvas { get; init; }
}

public class SkiaCanvas : Control
{
    private class SkiaCallbackRenderOperation : ICustomDrawOperation
    {
        public Action<SKCanvas>? RenderCall;
        public Rect Bounds { get; set; }

        public void Dispose()
        {
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public void Render(ImmediateDrawingContext context)
        {
            // This method is reached only when resizing the window or opening a menuitem over the control
            // No calls from Render, invoked via custom.Render reach this
            if (!context.TryGetFeature<ISkiaSharpApiLeaseFeature>(out var leaseFeature))
                throw new Exception("SkiaSharp is not supported.");

            using var lease = leaseFeature.Lease();

            RenderCall?.Invoke(lease.SkCanvas);
        }
    }

    public event Action<SkiaRenderEventArgs>? RenderSkia;
    private readonly SkiaCallbackRenderOperation _skiaCallbackRenderOperation;

    public SkiaCanvas()
    {
        _skiaCallbackRenderOperation = new SkiaCallbackRenderOperation
        {
            RenderCall = canvas =>
            {
                RenderSkia?.Invoke(new SkiaRenderEventArgs
                {
                    Sender = this,
                    Canvas = canvas
                });
            }
        };
        ClipToBounds = true;
    }


    public override void Render(DrawingContext context)
    {
        // This method (Render) is called at refresh rate, everything is fine

        _skiaCallbackRenderOperation.Bounds = new Rect(0, 0, DesiredSize.Width, DesiredSize.Height);

        // Jump into SkiaCallbackRenderOperation.Render
        context.Custom(_skiaCallbackRenderOperation);

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}