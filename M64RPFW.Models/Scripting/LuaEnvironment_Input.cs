using NLua;
// ReSharper disable UnusedMember.Local

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LuaFunction("input.get")]
    private LuaTable GetInput()
    {
        _lua.NewTable("___input");
        var table = _lua.GetTable("___input");
        table["xmouse"] = (int)_frontendScriptingService.PointerPosition.X;
        table["ymouse"] = (int)_frontendScriptingService.PointerPosition.Y;

        if (_frontendScriptingService.IsPrimaryPointerButtonHeld)
        {
            // otherwise, leave dont even make a table entry for it
            table["leftclick"] = true;
        }

        // TODO: implement keys
        return table;
    }
}