using System.Diagnostics;
using System.Reflection;
using M64RPFW.Services;
using NLua;

namespace M64RPFW.Models.Scripting;

public class LuaEnvironment : IDisposable
{
    private readonly Lua _lua;
    private readonly IFrontendScriptingService _frontendScriptingService;
    
    public LuaEnvironment(IFrontendScriptingService frontendScriptingService)
    {
        _frontendScriptingService = frontendScriptingService;
        _lua = new Lua();
        _lua.RegisterFunction("print", this, typeof(LuaEnvironment).GetMethod("Print", BindingFlags.Instance | BindingFlags.NonPublic));
    }

    public void Run(string path)
    {
        _lua.DoFile(path);
    }

    public void Dispose()
    {
        _lua.Dispose();
    }

    #region Function Registry

    private void Print(string value)
    {
        _frontendScriptingService.Print(value);
    }

    #endregion
}