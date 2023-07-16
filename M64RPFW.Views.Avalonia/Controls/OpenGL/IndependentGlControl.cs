using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Views.Avalonia.Controls.Helpers;

namespace M64RPFW.Views.Avalonia.Controls.OpenGL;

public class IndependentGlControl : CompositionControl
{
    private IOpenGlTextureSharingRenderInterfaceContextFeature? _glSharing;
    private IndependentGlControlResources? _resources;
    private Action _doPresent;

    private int _runCompositionLoop;

    private int _depthSize = 0;
    private bool _vSync = false;
    private int _requestedGlVersionMajor = 3;
    private int _requestedGlVersionMinor = 3;
    private GlProfileType _requestedGlProfileType = GlProfileType.OpenGL;

    public static readonly DirectProperty<IndependentGlControl, int> DepthSizeProperty = 
        AvaloniaProperty.RegisterDirect<IndependentGlControl, int>(nameof(DepthSize), c => c._depthSize, (c, value) => c._depthSize = value);
    public static readonly DirectProperty<IndependentGlControl, bool> VSyncProperty = 
        AvaloniaProperty.RegisterDirect<IndependentGlControl, bool>(nameof(VSync), c => c.VSync, (c, value) => c.VSync = value);
    public static readonly DirectProperty<IndependentGlControl, int> RequestedGlVersionMajorProperty = 
        AvaloniaProperty.RegisterDirect<IndependentGlControl, int>(nameof(RequestedGlVersionMajor), c => c._requestedGlVersionMajor, (c, value) => c._requestedGlVersionMajor = value);
    public static readonly DirectProperty<IndependentGlControl, int> RequestedGlVersionMinorProperty = 
        AvaloniaProperty.RegisterDirect<IndependentGlControl, int>(nameof(RequestedGlVersionMinor), c => c._requestedGlVersionMinor, (c, value) => c._requestedGlVersionMinor = value);
    public static readonly DirectProperty<IndependentGlControl, GlProfileType> RequestedGlProfileTypeProperty = 
        AvaloniaProperty.RegisterDirect<IndependentGlControl, GlProfileType>(nameof(RequestedGlProfileType), c => c._requestedGlProfileType, (c, value) => c._requestedGlProfileType = value);
    
    
    
    public IndependentGlControl()
    {
        _doPresent = DoPresent;
    }

    public int DepthSize
    {
        get => _depthSize;
        set => SetAndRaise(DepthSizeProperty, ref _depthSize, value);
    }
    public bool VSync
    {
        get => _vSync;
        set => SetAndRaise(VSyncProperty, ref _vSync, value);
    }
    public int RequestedGlVersionMajor
    {
        get => _requestedGlVersionMajor;
        set => SetAndRaise(RequestedGlVersionMajorProperty, ref _requestedGlVersionMajor, value);
    }
    public int RequestedGlVersionMinor
    {
        get => _requestedGlVersionMinor;
        set => SetAndRaise(RequestedGlVersionMinorProperty, ref _requestedGlVersionMinor, value);
    }
    public GlProfileType RequestedGlProfileType
    {
        get => _requestedGlProfileType;
        set => SetAndRaise(RequestedGlProfileTypeProperty, ref _requestedGlProfileType, value);
    }

    public GlVersion ContextVersion => _resources!.ContextVersion;

    protected async override Task InitGpuResources(Compositor compositor, CompositionDrawingSurface surface, ICompositionGpuInterop interop)
    {
        _glSharing = await compositor.GetRenderInterfaceFeature<IOpenGlTextureSharingRenderInterfaceContextFeature>();
        if (!_glSharing.CanCreateSharedContext)
            throw new PlatformNotSupportedException("Shared context not supported on this platform");
        _resources = new IndependentGlControlResources(_glSharing, new GlVersion(_requestedGlProfileType, _requestedGlVersionMajor, _requestedGlVersionMinor));

        _runCompositionLoop = 1;
        Compositor!.RequestCompositionUpdate(_doPresent);
    }

    protected override Task FreeGpuResources()
    {
        _runCompositionLoop = 0;
        _resources?.Dispose();
        return Task.CompletedTask;
    }

#pragma warning disable CS8774
    [MemberNotNull(nameof(_glSharing))]
    [MemberNotNull(nameof(_resources))]
    private void CheckInitialized()
    {
        if (InitTask is not { Status: TaskStatus.RanToCompletion })
        {
            throw new InvalidOperationException("Control must be initialized");
        }
    }
 #pragma warning restore CS8774

    private async void DoPresent()
    {
        CheckInitialized();
        try
        {
            await _resources.PresentNext(Interop!, Surface!);
        }
        catch (Exception e)
        {
            Mupen64Plus.Log(Mupen64Plus.LogSources.Vidext, Mupen64PlusTypes.MessageLevel.Error, $"DoPresent() -> {e}");
        }
        if (_runCompositionLoop != 0)
            Compositor!.RequestCompositionUpdate(_doPresent);
    }

    public IntPtr GetProcAddress(string sym)
    {
        CheckInitialized();
        return _resources.GetProcAddress(sym);
    }

    public IDisposable MakeContextCurrent()
    {
        CheckInitialized();
        var res = _resources.MakeCurrent();
        _resources.InitBackBuffer(WindowSize, DepthSize);
        return res;
    }

    public void SwapBuffers()
    {
        CheckInitialized();
        _resources.SwapBuffers(WindowSize, DepthSize);
    }

    public uint RenderFBO
    {
        get
        {
            CheckInitialized();
            return _resources.FBO ?? 0;
        }
    }
}