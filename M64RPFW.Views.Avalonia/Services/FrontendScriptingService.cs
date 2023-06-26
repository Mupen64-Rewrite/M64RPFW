using M64RPFW.Services;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia.Services;

// NOTE: This reaches its tentacles into view state, since it's a monolithic multi-responsibility service
// Don't attempt to clean up until an acceptable level of API compatibility and maturity is reached
public class FrontendScriptingService : IFrontendScriptingService
{
    private readonly LuaWindow _luaWindow;
    
    public FrontendScriptingService(LuaWindow luaWindow)
    {
        _luaWindow = luaWindow;
    }

    public void Print(string value)
    {
        _luaWindow.Print(value);
    }
}