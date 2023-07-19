using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using NLua;

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    
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
}