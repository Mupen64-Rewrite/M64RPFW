using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LuaFunction("emu.set_renderer")]
    private void SetRenderer(int renderer)
    {
        // We won't support the legacy WGui APIs.
    }
    
    [LuaFunction("emu.atvi")]
    private void RegisterAtVi(LuaFunction luaFunction)
    {
        _viCallback = luaFunction;
    }

    [LuaFunction("emu.atstop")]
    private void RegisterAtStop(LuaFunction luaFunction)
    {
        _stopCallback = luaFunction;
    }
    
    [LuaFunction("emu.atinput")]
    private void RegisterAtInput(LuaFunction luaFunction)
    {
        _inputCallback = luaFunction;
    }

    [LuaFunction("emu.atupdatescreen")]
    private void RegisterAtUpdateScreen(LuaFunction luaFunction)
    {
        _updateScreenCallback = luaFunction;
    }
    
    [LuaFunction("emu.framecount")]
    private int GetFrameIndex()
    {
        return _frameIndex;
    }

    [LuaFunction("emu.pause")]
    private void Pause()
    {
        Mupen64Plus.Pause();
    }

    [LuaFunction("emu.getpause")]
    private bool GetPause()
    {
        return Mupen64Plus.CoreStateQuery(Mupen64PlusTypes.CoreParam.EmuState) == (int)Mupen64PlusTypes.EmuState.Paused;
    }

    [LuaFunction("emu.isreadonly")]
    private bool GetVcrReadOnly()
    {
        return Mupen64Plus.VCR_DisableWrites;
    }
}