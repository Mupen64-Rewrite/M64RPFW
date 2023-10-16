using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    private void RegisterCallback(ICollection<LuaFunction> callbacks, LuaFunction callback)
    {
        // optional 2nd parameter: unregister?
        if (_lua.State.ToBoolean(2))
        {
            _lua.State.Pop(1);
            callbacks.Remove(callback);
        }
        else
        {
            if (_lua.State.GetTop() == 2)
            {
                _lua.State.Pop(1);
            }
            callbacks.Add(callback);
        }
    } 
    
    [LibFunction("emu.set_renderer")]
    private void SetRenderer(int renderer)
    {
        // We won't support the legacy WGui APIs.
    }
    
    [LibFunction("emu.atvi")]
    private void RegisterAtVi(LuaFunction luaFunction)
    {
        RegisterCallback(_viCallbacks, luaFunction);
    }

    [LibFunction("emu.atstop")]
    private void RegisterAtStop(LuaFunction luaFunction)
    {
        RegisterCallback(_stopCallbacks, luaFunction);
    }
    
    [LibFunction("emu.atinput")]
    private void RegisterAtInput(LuaFunction luaFunction)
    {
        RegisterCallback(_inputCallbacks, luaFunction);
    }

    [LibFunction("emu.atupdatescreen")]
    private void RegisterAtUpdateScreen(LuaFunction luaFunction)
    {
        RegisterCallback(_updateScreenCallbacks, luaFunction);
    }
    
    [LibFunction("emu.atwindowmessage")]
    private void RegisterAtWindowMessage(LuaFunction luaFunction)
    {
        // impossible to implement accurately with xplat support
    }
    
    [LibFunction("emu.atloadstate")]
    private void RegisterAtLoadState(LuaFunction luaFunction)
    {
        // TODO: implement
    }
    
    [LibFunction("emu.atreset")]
    private void RegisterAtReset(LuaFunction luaFunction)
    {
        // TODO: implement
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