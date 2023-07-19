using M64RPFW.Services.Abstractions;
using NLua;

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    private LuaTable GetWindowSize()
    {
        _lua.NewTable("dimensions");
        var table = _lua.GetTable("dimensions");
        var winSize = _windowSizingService.GetWindowSize();
        table["width"] = (int) winSize.Width;
        table["height"] = (int) winSize.Height;
        return table;
    }

    private void SetWindowSize(int width, int height)
    {
        _windowSizingService.SizeToFit(new WindowSize(width, height), false);
    }
}