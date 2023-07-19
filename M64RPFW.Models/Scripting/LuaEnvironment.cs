using System.Diagnostics;
using System.Reflection;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using NLua;
using NLua.Exceptions;
using SkiaSharp;
using SkiaExtensions = M64RPFW.Models.Scripting.Extensions.SkiaExtensions;

namespace M64RPFW.Models.Scripting;

public class LuaEnvironment : IDisposable
{
    private static int _frameIndex;
    private static List<LuaEnvironment> ActiveLuaEnvironments { get; } = new();
    private const BindingFlags EnvironmentBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private readonly Lua _lua;
    private readonly IFrontendScriptingService _frontendScriptingService;
    private readonly IWindowSizingService _windowSizingService;
    private readonly string _path;

    public event Action<bool>? StateChanged;

    private SKCanvas? _skCanvas;
    private LuaFunction? _viCallback;
    private LuaFunction? _stopCallback;
    private LuaFunction? _updateScreenCallback;

    static LuaEnvironment()
    {
        Debug.Print("Hooking Lua functionality to core...");
        Mupen64Plus.FrameComplete += (_, i) =>
        {
            ForEachEnvironment(x => x._viCallback?.Call());
            _frameIndex = i;
        };
    }

    private static void ForEachEnvironment(Action<LuaEnvironment> action)
    {
        ActiveLuaEnvironments.ForEach(action);
    }

    public LuaEnvironment(IFrontendScriptingService frontendScriptingService,
        string path)
    {
        const BindingFlags environmentBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        _frontendScriptingService = frontendScriptingService;
        _path = path;

        _lua = new Lua();
        // TODO: attribute-based registration
        _lua.RegisterFunction("print", _frontendScriptingService,
            typeof(IFrontendScriptingService).GetMethod(nameof(IFrontendScriptingService.Print)));
        _lua.RegisterFunction("stop", this, typeof(LuaEnvironment).GetMethod(nameof(Stop), environmentBindingFlags));
        _lua.RegisterFunction("_atvi", this,
            typeof(LuaEnvironment).GetMethod(nameof(RegisterAtVi), environmentBindingFlags));
        _lua.RegisterFunction("_atupdatescreen", this,
            typeof(LuaEnvironment).GetMethod(nameof(RegisterAtUpdateScreen), environmentBindingFlags));

        _lua.RegisterFunction("_atstop", this,
            typeof(LuaEnvironment).GetMethod(nameof(RegisterAtStop), environmentBindingFlags));
        _lua.RegisterFunction("_framecount", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetFrameIndex), environmentBindingFlags));
        _lua.RegisterFunction("_pause", this,
            typeof(LuaEnvironment).GetMethod(nameof(Pause), environmentBindingFlags));
        _lua.RegisterFunction("_getpause", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetPause), environmentBindingFlags));
        _lua.RegisterFunction("_isreadonly", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetVcrReadOnly), environmentBindingFlags));
        _lua.RegisterFunction("_info", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetWindowSize), environmentBindingFlags));
        _lua.RegisterFunction("_resize", this,
            typeof(LuaEnvironment).GetMethod(nameof(SetWindowSize), environmentBindingFlags));
        _lua.RegisterFunction("_get", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetInput), environmentBindingFlags));

        _lua.RegisterFunction("_fill_rectangle", this,
            typeof(LuaEnvironment).GetMethod(nameof(FillRectangle), environmentBindingFlags));
        _lua.RegisterFunction("_draw_rectangle", this,
            typeof(LuaEnvironment).GetMethod(nameof(DrawRectangle), environmentBindingFlags));
        _lua.RegisterFunction("_fill_ellipse", this,
            typeof(LuaEnvironment).GetMethod(nameof(FillEllipse), environmentBindingFlags));
        _lua.RegisterFunction("_draw_ellipse", this,
            typeof(LuaEnvironment).GetMethod(nameof(DrawEllipse), environmentBindingFlags));
        _lua.RegisterFunction("_draw_line", this,
            typeof(LuaEnvironment).GetMethod(nameof(DrawLine), environmentBindingFlags));
        _lua.RegisterFunction("_draw_text", this,
            typeof(LuaEnvironment).GetMethod(nameof(DrawText), environmentBindingFlags));
        _lua.RegisterFunction("_get_text_size", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetTextSize), environmentBindingFlags));
        _lua.RegisterFunction("_push_clip", this,
            typeof(LuaEnvironment).GetMethod(nameof(PushClip), environmentBindingFlags));
        _lua.RegisterFunction("_pop_clip", this,
            typeof(LuaEnvironment).GetMethod(nameof(PopClip), environmentBindingFlags));
        _lua.RegisterFunction("_fill_rounded_rectangle", this,
            typeof(LuaEnvironment).GetMethod(nameof(FillRoundedRectangle), environmentBindingFlags));
        _lua.RegisterFunction("_draw_rounded_rectangle", this,
            typeof(LuaEnvironment).GetMethod(nameof(DrawRoundedRectangle), environmentBindingFlags));
        _lua.RegisterFunction("_load_image", this,
            typeof(LuaEnvironment).GetMethod(nameof(LoadImage), environmentBindingFlags));
        _lua.RegisterFunction("_free_image", this,
            typeof(LuaEnvironment).GetMethod(nameof(FreeImage), environmentBindingFlags));
        _lua.RegisterFunction("_draw_image", this,
            typeof(LuaEnvironment).GetMethod(nameof(DrawImage), environmentBindingFlags));
        _lua.RegisterFunction("_get_image_info", this,
            typeof(LuaEnvironment).GetMethod(nameof(GetImageInfo), environmentBindingFlags));

        // HACK: NLua doesn't walk the virtual tree when registering functions to ensure validity of operations, so we have to create
        // sub-table functions as weirdly named globals and then execute code to properly set up the tables
        _lua.DoString(@"
            __dummy = function() print('Function not implemented in M64RPFW') end
            emu = {
                console = __dummy,
                debugview = __dummy,
                statusbar = __dummy,
                atvi = _atvi,
                atupdatescreen = _atupdatescreen,
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
            memory = {
                inttofloat = __dummy,
                inttodouble = __dummy,
                floattoint = __dummy,
                doubletoint = __dummy,
                qwordtonumber = __dummy,
                readbytesigned = __dummy,
                readbyte = __dummy,
                readwordsigned = __dummy,
                readword = __dummy,
                readdwordsigned = __dummy,
                readdword = __dummy,
                readqwordsigned = __dummy,
                readqword = __dummy,
                readfloat = __dummy,
                readdouble = __dummy,
                readsize = __dummy,
                writebyte = __dummy,
                writeword = __dummy,
                writedword = __dummy,
                writeqword = __dummy,
                writedouble = __dummy,
                writesize = __dummy,
            }
            wgui = {
                fill_rectangle = _fill_rectangle,
                draw_rectangle = _draw_rectangle,
                fill_ellipse = _fill_ellipse,
                draw_ellipse = _draw_ellipse,
                draw_line = _draw_line,
                draw_text = _draw_text,
                get_text_size = _get_text_size,
                push_clip = _push_clip,
                pop_clip = _pop_clip,
                fill_rounded_rectangle = _fill_rounded_rectangle,
                draw_rounded_rectangle = _draw_rounded_rectangle,
                load_image = _load_image,
                free_image = _free_image,
                draw_image = _draw_image,
                get_image_info = _get_image_info,
                set_text_antialias_mode = __dummy,
                set_antialias_mode = __dummy,
                gdip_fillpolygona = __dummy,
                info = _info,
                resize = _resize,
            }
            input = {
                get = _get,
                diff = __dummy,
                prompt = __dummy,
                get_key_name_text = __dummy,
                map_virtual_key_ex = __dummy,
            }
            joypad = {
                get = __dummy,
                set = __dummy,
                reegister = __dummy,
                count = __dummy,
            }
            movie = {
                playmovie = __dummy,
                stopmovie = __dummy,
                getmoviefilename = __dummy,
            }
            savestate = {
                savefile = __dummy,
                loadfile = __dummy,
            }
            iohelper = {
                filedialog = __dummy,
            }
            -- replace with encoder??
            avi = {
                startcapture = __dummy,
                stopcapture = __dummy,
            }
        ");
    }

    /// <summary>
    ///     Runs the lua script from the current path
    /// </summary>
    /// <returns>
    ///     Whether execution succeeded initially
    ///     NOTE: The script may fail later at runtime
    /// </returns>
    public bool Run()
    {
        ActiveLuaEnvironments.Add(this);
        StateChanged?.Invoke(true);

        try
        {
            // synchronously executes the entire file, but doesn't destroy the environment after finishing
            // instead, execution sleeps and is allowed to jump into callbacks arbitrarily on the lua thread 
            // be careful:
            // NOTE: some calls from lua side might arrive on another thread 
            _lua.DoFile(_path);
        }
        catch (LuaScriptException e)
        {
            _frontendScriptingService.Print($"{e.Source} {e.Message}");
            AtStop();
            return false;
        }

        return true;
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

    private void AtUpdateScreen(SKCanvas canvas)
    {
        // lua side can only issue drawcalls during updatescreen, anytime else it should be ignored (same as old mupen)
        _skCanvas = canvas;
        _updateScreenCallback?.Call();
        _skCanvas = null;
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

    private void RegisterAtUpdateScreen(LuaFunction luaFunction)
    {
        _updateScreenCallback = luaFunction;
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
    
    private LuaTable GetInput()
    {
        // TODO: implement
        _lua.NewTable("input");
        var table = _lua.GetTable("input");
        table["xmouse"] = 0;
        table["ymouse"] = 0;
        return table;
    }

    private LuaTable GetWindowSize()
    {
        _lua.NewTable("dimensions");
        var table = _lua.GetTable("dimensions");
        var winSize = _frontendScriptingService.WindowSizingService.GetWindowSize();
        table["width"] = (int)winSize.Width;
        table["height"] = (int)winSize.Height;
        return table;
    }

    private void SetWindowSize(int width, int height)
    {
        _frontendScriptingService.WindowSizingService.SizeToFit(new WindowSize(width, height), false);
    }

    private void FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    private void DrawRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, float thickness)
    {
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    private void DrawEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha, float thickness)
    {
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void DrawLine(float x0, float y0, float x1, float y1, float red, float green, float blue, float alpha,
        float thickness)
    {
        _skCanvas?.DrawLine(x0, y0, x1, y1, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void DrawText(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, string text, string fontName, float fontSize, int fontWeight, int fontStyle,
        int horizontalAlignment, int verticalAlignment, int options)
    {
        // TODO: implement
    }

    private LuaTable GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        // TODO: implement
        _lua.NewTable("text_size");
        var table = _lua.GetTable("text_size");
        table["width"] = 0;
        table["height"] = 0;
        return table;
    }

    private void PushClip(float x, float y, float right, float bottom)
    {
        // TODO: implement
    }

    private void PopClip()
    {
        // TODO: implement
    }

    private void FillRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    private void DrawRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha, float thickness)
    {
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void LoadImage(string path, string identifier)
    {
        // TODO: implement
    }

    private void FreeImage()
    {
        // TODO: implement
    }

    private void DrawImage(float sourceX, float sourceY, float sourceRight, float sourceBottom, float destinationX,
        float destinationY, float destinationRight, float destinationBottom,
        string identifier, float opacity, int interpolation)
    {
        // TODO: implement
    }

    private LuaTable GetImageInfo()
    {
        // TODO: implement
        _lua.NewTable("image_info");
        var table = _lua.GetTable("image_info");
        table["width"] = 0;
        table["height"] = 0;
        return table;
    }
    
    #endregion
}