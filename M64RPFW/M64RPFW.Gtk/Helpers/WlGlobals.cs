using System;
using WaylandSharp;

namespace M64RPFW.Gtk.Helpers;

public static class WlGlobals
{
    public static void Init(WlDisplay display)
    {
        if (!(_display is null))
            return;
        
        _display = display;
        WlRegistry registry = display.GetRegistry();
        registry.Global += (sender, args) =>
        {
            if (args.Interface == "wl_compositor")
            {
                _compositor = registry.Bind<WlCompositor>(args.Name, args.Interface, args.Version);
            }

            if (args.Interface == "wl_subcompositor")
            {
                _subcompositor = registry.Bind<WlSubcompositor>(args.Name, args.Interface, args.Version);
            }
        };
    }

    private static InvalidOperationException NotInitializedError()
    {
        return new InvalidOperationException("WlGlobals.Init() has not been called yet!");
    }

    private static WlDisplay? _display;
    private static WlCompositor? _compositor;
    private static WlSubcompositor? _subcompositor;

    public static WlDisplay Display => _display ?? throw NotInitializedError();
    public static WlCompositor Compositor => _compositor ?? throw NotInitializedError();
    public static WlSubcompositor Subcompositor => _subcompositor ?? throw NotInitializedError();
}