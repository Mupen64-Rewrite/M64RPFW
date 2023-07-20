using M64RPFW.Models.Emulation;
using NLua;
// ReSharper disable UnusedMember.Local

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LuaFunction("memory.read_byte_signed")]
    private int ReadByteSigned(uint addr)
    {
        return (sbyte) Mupen64Plus.RDRAM_Read8(addr);
    }

    [LuaFunction("memory.read_byte")]
    private uint ReadByte(uint addr)
    {
        return Mupen64Plus.RDRAM_Read8(addr);
    }

    [LuaFunction("memory.read_word_signed")]
    private int ReadWordSigned(uint addr)
    {
        return (short) Mupen64Plus.RDRAM_Read16(addr);
    }

    [LuaFunction("memory.read_word")]
    private uint ReadWord(uint addr)
    {
        return Mupen64Plus.RDRAM_Read16(addr);
    }

    [LuaFunction("memory.read_dword_signed")]
    private int ReadDwordSigned(uint addr)
    {
        return (int) Mupen64Plus.RDRAM_Read32(addr);
    }

    [LuaFunction("memory.read_dword")]
    private uint ReadDword(uint addr)
    {
        return Mupen64Plus.RDRAM_Read32(addr);
    }

    [LuaFunction("memory.read_qword_signed")]
    private LuaTable ReadQwordSigned(uint addr)
    {
        _lua.NewTable("__2qword_signed_read");
        var table = _lua.GetTable("__2qword_signed_read");
        var value = Mupen64Plus.RDRAM_Read64(addr);
        table[0] = (uint) (value >> 32);
        table[1] = (uint) value;
        return table;
    }

    [LuaFunction("memory.read_qword")]
    private LuaTable ReadQword(uint addr)
    {
        _lua.NewTable("__2qword_read");
        var table = _lua.GetTable("__2qword_read");
        var value = Mupen64Plus.RDRAM_Read64(addr);
        table[0] = (uint) (value >> 32);
        table[1] = (uint) value;
        return table;
    }
}