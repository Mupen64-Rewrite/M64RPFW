using NLua;

namespace M64RPFW.Models.Scripting.Extensions;

/// <summary>
/// A class providing extension methods and helpers for <see cref="Lua"/>
/// </summary>
public static class LuaExtensions
{
    /// <summary>
    /// Converts a <paramref name="luaTable"/> to a <see cref="string"/>
    /// </summary>
    /// <param name="luaTable">The <see cref="LuaTable"/> to be converted</param>
    /// <returns>The <see cref="string"/> representation of <paramref name="luaTable"/></returns>
    public static string ToString(this LuaTable luaTable)
    {
        var value = "";
        var keys = luaTable.Keys.Cast<object>().ToArray();
        var values = luaTable.Values.Cast<object>().ToArray();

        for (int i = 0; i < keys.Length; i++)
        {
            value += $"{keys[i]} | {values[i]}\n";
        }

        return value;
    }

    public static LuaTable NewUnnamedTable(this Lua lua)
    {
        return (LuaTable) lua.DoString("return {}")[0];
    }

    public static long GetLength(this Lua lua, LuaTable table)
    {
        var luaState = lua.State;

        luaState.CheckStack(2, "Can't check table length");
        
        lua.Push(table);
        luaState.PushLength(-1);
        long result = luaState.ToInteger(-1);
        
        luaState.Pop(2);
        return result;
    }
}