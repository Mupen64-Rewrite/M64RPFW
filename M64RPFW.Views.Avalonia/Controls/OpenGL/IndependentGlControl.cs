using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using M64RPFW.Views.Avalonia.Controls.Helpers;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public class IndependentGlControl : CompositionControl
{
    private IOpenGlTextureSharingRenderInterfaceContextFeature? _glSharing;
    private IGlContext? _context;
    
    protected async override Task InitGpuResources(Compositor compositor, CompositionDrawingSurface surface, ICompositionGpuInterop interop)
    {
        _glSharing = await compositor.GetRenderInterfaceFeature<IOpenGlTextureSharingRenderInterfaceContextFeature>();
        if (!_glSharing.CanCreateSharedContext)
            throw new PlatformNotSupportedException("Shared context not supported on this platform");

        _context = _glSharing.CreateSharedContext();
    }

    protected override Task FreeGpuResources()
    {
        _context?.Dispose();
        return Task.CompletedTask;
    }
    
#pragma warning disable CS8774
    [MemberNotNull(nameof(_glSharing))]
    [MemberNotNull(nameof(_context))]
    private void CheckInitialized()
    {
        if (InitTask is not { Status: TaskStatus.RanToCompletion })
        {
            throw new InvalidOperationException("Control must be initialized");
        }
    }
 #pragma warning restore CS8774

    public IDisposable MakeContextCurrent()
    {
        CheckInitialized();

        return _context.MakeCurrent();
    }

    public void SwapBuffers()
    {
        
    }
}