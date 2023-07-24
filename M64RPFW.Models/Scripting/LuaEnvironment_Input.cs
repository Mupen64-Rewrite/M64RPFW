using M64RPFW.Models.Scripting.Extensions;
using M64RPFW.Services.Abstractions;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LuaFunction("input.get")]
    private LuaTable GetInput()
    {
        var table = _lua.NewUnnamedTable();
        var winService = _frontendScriptingService.WindowAccessService;
        table["xmouse"] = (int)winService.PointerPosition.X;
        table["ymouse"] = (int)winService.PointerPosition.Y;

        if ((winService.PointerButtons & MouseButtonMask.Primary) != 0)
        {
            // otherwise, leave dont even make a table entry for it
            table["leftclick"] = true;
        }

        // TODO: implement keys
        return table;
    }
}