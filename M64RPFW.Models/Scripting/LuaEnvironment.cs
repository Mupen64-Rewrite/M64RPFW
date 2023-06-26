using System.Diagnostics;
using System.Reflection;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using NLua;
using NLua.Exceptions;

namespace M64RPFW.Models.Scripting;

public class LuaEnvironment : IDisposable
{
    private static int _frameIndex;
    private static List<LuaEnvironment> ActiveLuaEnvironments { get; } = new();
    private const BindingFlags EnvironmentBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private readonly Lua _lua;
    private readonly IFrontendScriptingService _frontendScriptingService;
    private readonly string _path;

    public event Action<bool>? StateChanged;

    private LuaFunction? _viCallback;
    private LuaFunction? _stopCallback;


    static LuaEnvironment()
    {
        Debug.Print("Hooking Lua functionality to core...");
        Mupen64Plus.FrameComplete += (_, i) =>
        {
            ForEachEnvironment(x => x._viCallback?.Call());
            _frameIndex = i;
        };
    }

    public static void ForEachEnvironment(Action<LuaEnvironment> action)
    {
        ActiveLuaEnvironments.ForEach(action);
    }

    public LuaEnvironment(IFrontendScriptingService frontendScriptingService, string path)
    {
        _frontendScriptingService = frontendScriptingService;
        _path = path;

        _lua = new Lua();
        // TODO: attribute-based registration
        _lua.RegisterFunction("print", _frontendScriptingService,
            typeof(IFrontendScriptingService).GetMethod(nameof(IFrontendScriptingService.Print)));
        _lua.RegisterFunction("stop", this, typeof(LuaEnvironment).GetMethod(nameof(Stop), EnvironmentBindingFlags));
        _lua.RegisterFunction("_atvi", this,
            typeof(LuaEnvironment).GetMethod(nameof(RegisterAtVi), EnvironmentBindingFlags));

        _lua.RegisterFunction("_atstop", this,
            typeof(LuaEnvironment).GetMethod(nameof(RegisterAtStop), EnvironmentBindingFlags));
        _lua.RegisterFunction("_framecount", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetFrameIndex), EnvironmentBindingFlags));
        _lua.RegisterFunction("_pause", this,
            typeof(LuaEnvironment).GetMethod(nameof(Pause), EnvironmentBindingFlags));
        _lua.RegisterFunction("_getpause", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetPause), EnvironmentBindingFlags));
        _lua.RegisterFunction("_isreadonly", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetVcrReadOnly), EnvironmentBindingFlags));
        
        // HACK: NLua doesn't walk the virtual tree when registering functions to ensure validity of operations, so we have to create
        // sub-table functions as weirdly named globals and then execute code to properly set up the tables
        _lua.DoString(@"
            __dummy = function() end
            emu = {
                console = __dummy,
                debugview = __dummy,
                statusbar = __dummy,
                atvi = _atvi,
                atupdatescreen = __dummy,
                atinput = __dummy,
                atstop = _atstop,
                atwindowmessage = __dummy,
                atstopmovie = __dummy,
                atloadstate = __dummy,
                atsavestate = __dummy,
                atreset = __dummy,
                framecount = _framecount,
                samplecount = __dummy,
                inputcount = __dummy,
                getversion = __dummy,
                pause = _pause,
                getpause = _getpause,
                getspeed = __dummy,
                speed = __dummy,
                speedmode = __dummy,
                setgfx = __dummy,
                getaddress = __dummy,
                isreadonly = _isreadonly,
                getsystemmetrics = __dummy,
                ismainwindowinforeground = __dummy,
                screenshot = __dummy,
            }
        ");
    }

    public void Run()
    {
        ActiveLuaEnvironments.Add(this);
        StateChanged?.Invoke(true);

        try
        {
            _lua.DoFile(_path);
        }
        catch (LuaScriptException e)
        {
            _frontendScriptingService.Print($"{e.Source} {e.Message}");
            AtStop();
        }
    }

    public void Dispose()
    {
        ForEachEnvironment(x => x._stopCallback?.Call());
        AtStop();
        _lua.Dispose();
    }

    private void AtStop()
    {
        ActiveLuaEnvironments.Remove(this);
        StateChanged?.Invoke(false);
    }

    #region Function Registry

    private void Dummy()
    {
    }

    private void Stop()
    {
        _lua.State.Error("Execution terminated via stop()");
    }

    private void RegisterAtVi(LuaFunction luaFunction)
    {
        _viCallback = luaFunction;
    }

    private void RegisterAtStop(LuaFunction luaFunction)
    {
        _stopCallback = luaFunction;
    }

    private int GetFrameIndex()
    {
        return _frameIndex;
    }

    private void Pause()
    {
        Mupen64Plus.Pause();
    }

    private bool GetPause()
    {
        return Mupen64Plus.CoreStateQuery(Mupen64PlusTypes.CoreParam.EmuState) == (int)Mupen64PlusTypes.EmuState.Paused;
    }

    private bool GetVcrReadOnly()
    {
        return Mupen64Plus.VCR_DisableWrites;
    }

    #endregion
}