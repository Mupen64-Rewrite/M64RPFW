using M64RPFW.Models.Emulation;
using M64RPFW.Models.Emulation.Helpers;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    #region Integer reads

    [LibFunction("memory.readbytesigned")]
    private sbyte ReadByteSigned(uint addr)
    {
        try
        {
            return (sbyte) Mupen64Plus.RDRAM_Read8(addr);
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            return 0;
        }
    }

    [LibFunction("memory.readbyte")]
    private byte ReadByte(uint addr)
    {
        try
        {
            return Mupen64Plus.RDRAM_Read8(addr);
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            return 0;
        }
    }

    [LibFunction("memory.readwordsigned")]
    private short ReadWordSigned(uint addr)
    {
        try
        {
            return (short) Mupen64Plus.RDRAM_Read16(addr);
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            return 0;
        }
    }

    [LibFunction("memory.readword")]
    private ushort ReadWord(uint addr)
    {
        try
        {
            return Mupen64Plus.RDRAM_Read16(addr);
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            return 0;
        }
    }

    [LibFunction("memory.readdwordsigned")]
    private int ReadDwordSigned(uint addr)
    {
        try
        {
            return (int) Mupen64Plus.RDRAM_Read32(addr);
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            return 0;
        }
    }

    [LibFunction("memory.readdword")]
    private uint ReadDword(uint addr)
    {
        try
        {
            return Mupen64Plus.RDRAM_Read32(addr);
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            return 0;
        }
    }

    [LibFunction("memory.readqwordsigned")]
    private LuaTable ReadQwordSigned(uint addr)
    {
        var table = _lua.NewUnnamedTable();
        try
        {
            var value = Mupen64Plus.RDRAM_Read64(addr);
            table[0] = (uint) (value >> 32);
            table[1] = (uint) value;
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            table[0] = 0;
            table[1] = 0;
        }
        return table;
    }

    [LibFunction("memory.readqword")]
    private LuaTable ReadQword(uint addr)
    {
        var table = _lua.NewUnnamedTable();
        try
        {
            var value = Mupen64Plus.RDRAM_Read64(addr);
            table[1] = (uint) (value >> 32);
            table[2] = (uint) value;
        }
        catch (MupenException e)
        {
            if (e.ErrorCode != Error.InputNotFound)
                throw;
            table[1] = 0;
            table[2] = 0;
        }
        return table;
    }

    #endregion

    #region Integer writes

    [LibFunction("memory.writebyte")]
    private void WriteByte(uint addr, byte val)
    {
        Mupen64Plus.RDRAM_Write8(addr, val);
    }

    [LibFunction("memory.writeword")]
    private void WriteWord(uint addr, ushort val)
    {
        Mupen64Plus.RDRAM_Write16(addr, val);
    }

    [LibFunction("memory.writedword")]
    private void WriteDword(uint addr, uint val)
    {
        Mupen64Plus.RDRAM_Write32(addr, val);
    }

    [LibFunction("memory.writeqword")]
    private void WriteQword(uint addr, LuaTable val)
    {
        if (val[1] is not uint hi)
            throw new ArgumentException("value must be an array of two unsigned integers");
        if (val[2] is not uint lo)
            throw new ArgumentException("value must be an array of two unsigned integers");

        ulong binVal = (ulong) hi << 32 | lo;
        Mupen64Plus.RDRAM_Write64(addr, binVal);
    }

    #endregion

    #region FP read/write

    [LibFunction("memory.readfloat")]
    private float ReadFloat(uint addr)
    {
        uint val = Mupen64Plus.RDRAM_Read32(addr);
        return BitConverter.UInt32BitsToSingle(val);
    }

    [LibFunction("memory.readdouble")]
    private double ReadDouble(uint addr)
    {
        ulong val = Mupen64Plus.RDRAM_Read64(addr);
        return BitConverter.UInt64BitsToDouble(val);
    }

    [LibFunction("memory.writefloat")]
    private void WriteFloat(uint addr, float val)
    {
        uint bits = BitConverter.SingleToUInt32Bits(val);
        Mupen64Plus.RDRAM_Write32(addr, bits);
    }

    [LibFunction("memory.writedouble")]
    private void WriteDouble(uint addr, double val)
    {
        ulong bits = BitConverter.DoubleToUInt64Bits(val);
        Mupen64Plus.RDRAM_Write64(addr, bits);
    }

    #endregion
}