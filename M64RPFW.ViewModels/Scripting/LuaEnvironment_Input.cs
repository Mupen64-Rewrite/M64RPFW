using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("input.get")]
    private LuaTable GetInput()
    {
        var table = _lua.NewUnnamedTable();
        var winService = _luaInterfaceService;
        table["xmouse"] = winService.PointerPosition.X;
        table["ymouse"] = winService.PointerPosition.Y;

        if ((winService.PointerButtons & MouseButtonMask.Primary) != 0)
        {
            // otherwise, leave dont even make a table entry for it
            table["leftclick"] = true;
        }

        foreach (var key in winService.HeldKeys)
        {
            table[key] = true;
        }
        
        return table;
    }
}