using NLua;

namespace M64RPFW.ViewModels.Scripting.Extensions;

/// <summary>
/// A class providing extension methods and helpers for <see cref="NLua.Lua"/>
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

    /// <summary>
    /// Constructs a new empty Lua table.
    /// </summary>
    /// <param name="nLua">The NLua instance.</param>
    /// <returns>The new table.</returns>
    public static LuaTable NewUnnamedTable(this Lua nLua)
    {
        nLua.State.NewTable();
        return (LuaTable) nLua.Pop();
    }

    /// <summary>
    /// Returns the "length" of a table. Equivalent to <c>#table</c> on the Lua side.
    /// </summary>
    /// <param name="nLua">The NLua instance.</param>
    /// <param name="table">The table to check the length of.</param>
    /// <returns></returns>
    public static long GetLength(this Lua nLua, LuaTable table)
    {
        var lua = nLua.State;
        int top = lua.GetTop();

        lua.CheckStack(2, "Can't check table length");
        
        nLua.Push(table);
        lua.PushLength(-1);
        long result = lua.ToInteger(-1);
        
        lua.SetTop(top);
        return result;
    }
    
    /// <summary>
    /// Iterates a table with string keys and values.
    /// </summary>
    /// <param name="nLua">The NLua instance.</param>
    /// <param name="table">The table to process.</param>
    /// <param name="receivePair">A function which receives key-value pairs.</param>
    /// <exception cref="ArgumentException">If the provided table contains keys or values that are not strings.</exception>
    public static void IterateStringDict(this Lua nLua, LuaTable table, Action<string, string> receivePair)
    {
        KeraLua.Lua lua = nLua.State;
        int top = lua.GetTop();
        nLua.Push(table);
        
        lua.PushNil();
        while (lua.Next(-2))
        {
            if (!(lua.IsString(-2) && lua.IsString(-1)))
                throw new ArgumentException("Table contains non-string key or value");

            receivePair(lua.ToString(-2), lua.ToString(-1));
            
            lua.Pop(1);
        }
        
        lua.SetTop(top);
    }

    /// <summary>
    /// Iterates a table with integer keys and table values. Each table value contains a number at indexes 0 and 1.
    /// </summary>
    /// <param name="nLua">The NLua instance.</param>
    /// <param name="table">The table to process.</param>
    /// <param name="receiveIndexedPoint">A function which receives key-value pairs.</param>
    /// <exception cref="ArgumentException">If the provided table does not match the format specified in summary.</exception>
    public static void IteratePointList(this Lua nLua, LuaTable table, Action<long, double, double> receiveIndexedPoint)
    {
        KeraLua.Lua lua = nLua.State;
        int top = lua.GetTop();
        nLua.Push(table);
        
        // get table length
        lua.PushLength(-1);
        long len = lua.ToInteger(-1);
        lua.Pop(1);

        // Iterate over subtables
        for (long i = 1; i <= len; i++)
        {
            // push subtable
            lua.GetInteger(-1, i);
            // push point coords
            lua.GetInteger(-1, 1);
            lua.GetInteger(-2, 2);

            receiveIndexedPoint(i, lua.CheckNumber(-2), lua.CheckNumber(-1));
            
            // pop everything
            lua.Pop(3);
        }
        
        lua.SetTop(top);
    }
}