using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("wgui.info")]
    private LuaTable GetWindowSize()
    {
        var table = _lua.NewUnnamedTable();
        var winSize = _luaInterfaceService.GetWindowSize();
        table["width"] = winSize.Width;
        table["height"] = winSize.Height;
        return table;
    }

    [LibFunction("wgui.resize")]
    private void SetWindowSize(int width, int height)
    {
        _luaInterfaceService.SizeToFit(new WindowSize(width, height));
    }
}