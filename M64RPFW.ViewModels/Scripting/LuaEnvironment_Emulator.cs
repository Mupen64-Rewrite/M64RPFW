using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("emu.set_renderer")]
    private void SetRenderer(int renderer)
    {
        // We won't support the legacy WGui APIs.
    }
    
    [LibFunction("emu.atvi")]
    private void RegisterAtVi(LuaFunction luaFunction)
    {
        _viCallback = luaFunction;
    }

    [LibFunction("emu.atstop")]
    private void RegisterAtStop(LuaFunction luaFunction)
    {
        _stopCallback = luaFunction;
    }
    
    [LibFunction("emu.atinput")]
    private void RegisterAtInput(LuaFunction luaFunction)
    {
        _inputCallback = luaFunction;
    }

    [LibFunction("emu.atupdatescreen")]
    private void RegisterAtUpdateScreen(LuaFunction luaFunction)
    {
        _updateScreenCallback = luaFunction;
    }
    
    [LibFunction("emu.framecount")]
    private int GetFrameIndex()
    {
        return _frameIndex;
    }

    [LibFunction("emu.pause")]
    private void Pause()
    {
        Mupen64Plus.Pause();
    }

    [LibFunction("emu.getpause")]
    private bool GetPause()
    {
        return Mupen64Plus.CoreStateQuery(Mupen64PlusTypes.CoreParam.EmuState) == (int)Mupen64PlusTypes.EmuState.Paused;
    }

    [LibFunction("emu.isreadonly")]
    private bool GetVcrReadOnly()
    {
        return Mupen64Plus.VCR_DisableWrites;
    }
}