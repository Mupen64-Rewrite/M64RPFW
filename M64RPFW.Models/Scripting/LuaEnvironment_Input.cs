using NLua;

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    
    
    private LuaTable GetInput()
    {
        // TODO: implement
        _lua.NewTable("input");
        var table = _lua.GetTable("input");
        table["xmouse"] = 0;
        table["ymouse"] = 0;
        return table;
    }

}