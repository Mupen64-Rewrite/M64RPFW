using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using M64RPFW.Services.Abstractions;
using SkiaSharp;

namespace M64RPFW.Views.Avalonia.Controls;
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

    public event EventHandler<SkiaRenderEventArgs>? RenderSkia;
    private readonly SkiaCallbackRenderOperation _skiaCallbackRenderOperation;

    private bool _isOrange = true;

    public SkiaCanvas()
    {
        _skiaCallbackRenderOperation = new SkiaCallbackRenderOperation
        {
            RenderCall = canvas =>
            {
                if (RenderSkia != null)
                {
                    RenderSkia.Invoke(this, new SkiaRenderEventArgs
                    {
                        Canvas = canvas
                    });
                }
                else
                {
                    canvas.Clear(SKColors.Transparent);
                }
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

        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(_ => InvalidateVisual());
    }
}