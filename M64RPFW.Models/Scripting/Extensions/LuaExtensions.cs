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

    public static LuaTable NewUnnamedTable(this Lua nLua)
    {
        nLua.State.NewTable();
        return (LuaTable) nLua.Pop();
    }

    public static long GetLength(this Lua nLua, LuaTable table)
    {
        var lua = nLua.State;

        lua.CheckStack(2, "Can't check table length");
        
        nLua.Push(table);
        lua.PushLength(-1);
        long result = lua.ToInteger(-1);
        
        lua.Pop(2);
        return result;
    }
    
    /// <summary>
    /// Syntax sugar to get a table from another table.
    /// </summary>
    /// <param name="table">The table to check</param>
    /// <param name="key">The key to use</param>
    /// <returns>The value at <c>table[key]</c></returns>
    public static LuaTable GetTable(this LuaTable table, object key)
    {
        return (LuaTable) table[key];
    }
    
    /// <summary>
    /// Syntax sugar to get a number from a table.
    /// </summary>
    /// <param name="table">The table to check</param>
    /// <param name="key">The key to use</param>
    /// <returns>The value at <c>table[key]</c></returns>
    public static double GetNumber(this LuaTable table, object key)
    {
        return (double) table[key];
    }
}