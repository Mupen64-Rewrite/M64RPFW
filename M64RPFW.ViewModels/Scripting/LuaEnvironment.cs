using System.Diagnostics;
using System.Reflection;
using M64RPFW.Models.Emulation;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;
using NLua.Exceptions;
using SkiaSharp;

namespace M64RPFW.ViewModels.Scripting;

/// <summary>
/// Manages the lifetime of a Lua runtime.
/// </summary>
public partial class LuaEnvironment : IDisposable
{
    [AttributeUsage(AttributeTargets.Method)]
    private class LibFunctionAttribute : Attribute
    {
        public LibFunctionAttribute(string path)
        {
            Path = path;
        }
        
        public string Path { get; }
    }
    
    private static int _frameIndex;
    private static readonly List<LuaEnvironment> ActiveLuaEnvironments = new();

    private readonly Lua _lua;
    private readonly string _path;
    
    private readonly ILuaWindowService _luaWindowService;
    private readonly ILuaInterfaceService _luaInterfaceService;
    private readonly IFilePickerService _filePickerService;

    // Synchronization stuff
    private ReaderWriterLockSlim _luaLock;
    private bool _isActive;

    public event Action<bool>? StateChanged;

    private SKCanvas? _skCanvas;
    private List<LuaFunction> _viCallbacks = new();
    private List<LuaFunction> _inputCallbacks = new();
    private List<LuaFunction> _stopCallbacks = new();
    private List<LuaFunction> _updateScreenCallbacks = new();

    static LuaEnvironment()
    {
        Debug.Print("Hooking Lua functionality to core...");
        Mupen64Plus.FrameComplete += frameCount =>
        {
            ForEachEnvironment(env =>
            {
                if (!env._isActive)
                    return;
                using (env._luaLock.ReadLock())
                {
                    foreach (LuaFunction callback in env._viCallbacks)
                    {
                        callback.GuardedCall(env._lua);
                    }
                    foreach (LuaFunction callback in env._inputCallbacks)
                    {
                        callback.GuardedCall(env._lua);
                    }
                }
                if (!env._isActive)
                    env.TryDisposeLock();
            });
            _frameIndex = frameCount;
        };
    }

    private static void ForEachEnvironment(Action<LuaEnvironment> action)
    {
        try
        {
            ActiveLuaEnvironments.ForEach(action);
        }
        catch (InvalidOperationException)
        {
            // ignored, this might be a bit of a kludge
        }
    }

    public LuaEnvironment(string path, ILuaWindowService luaWindowService, ILuaInterfaceService luaInterfaceService, IFilePickerService filePickerService)
    {
        _luaWindowService = luaWindowService;
        _luaInterfaceService = luaInterfaceService;
        _filePickerService = filePickerService;
        _path = path;
        _luaInterfaceService.OnSkiaRender += AtUpdateScreen;

        _luaLock = new ReaderWriterLockSlim();
        _isActive = false;

        using (_luaLock.WriteLock())
        {
            _lua = new Lua();
            LuaRegisterTaggedFunctions();
            LoadDotnetClasses();
        }
    }
    
    /// <summary>
    /// Registers all functions tagged with <see cref="LibFunctionAttribute"/>.
    /// </summary>
    private void LuaRegisterTaggedFunctions()
    {
        // Register global functions and save the ones that belong in tables.
        // ====================================================================
        var tables = new SortedDictionary<string, List<(string src, MethodInfo name)>>();
        foreach (var method in typeof(LuaEnvironment).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (method.GetCustomAttribute<LibFunctionAttribute>() is not { } attr)
                continue;

            int splitPoint = attr.Path.LastIndexOf('.');
            string? luaNs = splitPoint != -1 ? attr.Path[..splitPoint] : null;
            string luaName = attr.Path[(splitPoint + 1)..];
            if (luaNs == null)
            {
                _lua.RegisterFunction(luaName, this, method);
            }
            else
            {
                // Save anything in a table to a sorted dictionary. The sorting ensures that
                // every table is always setup before its sub-tables.
                if (!tables.ContainsKey(luaNs))
                    tables[luaNs] = new List<(string name, MethodInfo func)>();
                tables[luaNs].Add((luaName, method));
            }
        }
        foreach ((string ns, var entries) in tables)
        {
            // We have to create the tables explicitly.
            _lua.NewTable(ns);
            foreach ((string src, MethodInfo func) in entries)
            {
                _lua.RegisterFunction($"{ns}.{src}", this, func);
            }
        }
    }

    /// <summary>
    /// Loads SkiaSharp's API into Lua.
    /// </summary>
    private void LoadDotnetClasses()
    {
        const string code =
            """
            luanet.load_assembly("SkiaSharp")
            skia = luanet.SkiaSharp
            import = function () end
            """;
        _lua.DoString(code);
    }
    
    /// <summary>
    ///     Runs the Lua script from the current path
    /// </summary>
    /// <returns>
    ///     Whether execution succeeded initially
    ///     NOTE: The script may fail later at runtime
    /// </returns>
    public bool Run()
    {
        lock (ActiveLuaEnvironments)
        {
            ActiveLuaEnvironments.Add(this);
        }
        StateChanged?.Invoke(true);

        try
        {
            // synchronously executes the entire file, but doesn't destroy the environment after finishing
            // instead, execution sleeps and is allowed to jump into callbacks arbitrarily on the lua thread 
            // be careful:
            // NOTE: some calls from lua side might arrive on another thread 
            using (_luaLock.ReadLock())
            {
                _isActive = true;
                _lua.DoFile(_path);
            }
        }
        catch (LuaScriptException e)
        {
            AtStop();
            _luaWindowService.Print($"{e.Source} {e.Message}");
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        if (!_isActive)
            return;
        using (_luaLock.WriteLock())
        {
            AtStop();
            _luaInterfaceService.OnSkiaRender -= AtUpdateScreen;
            foreach (var callback in _stopCallbacks)
            {
                callback.GuardedCall(_lua);
            }

            _stopCallbacks = null;
            _viCallbacks = null;
            _inputCallbacks = null;
            _updateScreenCallbacks = null;
            
            _lua.Dispose();
        }
        TryDisposeLock();
    }

    private void TryDisposeLock()
    {
        try
        {
            _luaLock.Dispose();
        }
        catch (SynchronizationLockException)
        {
            // ignored
        }
    }

    private void AtStop()
    {
        _isActive = false;
        lock (ActiveLuaEnvironments)
        {
            ActiveLuaEnvironments.Remove(this);
        }
        StateChanged?.Invoke(false);
    }

    private void AtUpdateScreen(object? sender, SkiaRenderEventArgs args)
    {
        // lua side can only issue drawcalls during updatescreen, anytime else it should be ignored (same as old mupen)
        if (!_isActive)
            return;
        using (_luaLock.ReadLock())
        {
            try
            {
                _skCanvas = args.Canvas;
                foreach (LuaFunction callback in _updateScreenCallbacks)
                {
                    callback.GuardedCall(_lua);
                }
            }
            finally
            {
                _skCanvas = null;
            }
        }
        if (!_isActive)
            TryDisposeLock();
    }

    #region Function Registry

    private void Dummy()
    {
    }

    [LibFunction("print")]
    private void Print(object? value)
    {
        var formatted = value switch
        {
            null => "nil",
            LuaTable luaTable => LuaExtensions.ToString(luaTable),
            _ => value.ToString()!
        };

        _luaWindowService.Print(formatted);
    }
    [LibFunction("stop")]
    private void Stop()
    {
        _lua.State.Error("Execution terminated via stop()");
    }
    #endregion
}