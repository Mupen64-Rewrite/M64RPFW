using M64RPFW.Models.Scripting.Extensions;
using M64RPFW.Services.Abstractions;
using NLua;

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("wgui.info")]
    private LuaTable GetWindowSize()
    {
        var table = _lua.NewUnnamedTable();
        var winSize = _luaInterfaceService.GetWindowSize();
        table["width"] = (int) winSize.Width;
        table["height"] = (int) winSize.Height;
        return table;
    }

    [LibFunction("wgui.resize")]
    private void SetWindowSize(int width, int height)
    {
        _luaInterfaceService.SizeToFit(new WindowSize(width, height));
    }
}