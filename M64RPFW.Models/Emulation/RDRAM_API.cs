namespace M64RPFW.Models.Emulation;

public unsafe partial class Mupen64Plus
{
    public static byte RDRAM_Read8(uint addr)
    {
        uint wordAddr = addr & ~3U;
        int shift = (int) ((3 - (addr & 3U)) * 8);

        uint word = 0;
        ThrowForError(_rdramReadAligned(wordAddr, &word));
        return (byte) ((word >> shift) & 0xFF);
    }

    public static void RDRAM_Write8(uint addr, byte val)
    {
        uint wordAddr = addr & ~3U;
        int shift = (int) ((3 - (addr & 3U)) * 8);
        
        ThrowForError(_rdramWriteAligned(wordAddr, (uint) val << shift, 0xFFU << shift));
    }

    public static ushort RDRAM_Read16(uint addr)
    {
        uint wordAddr = addr & ~3U;
        if ((addr & 3U) == 3)
        {
            uint word0 = 0, word1 = 0;
            ThrowForError(_rdramReadAligned(wordAddr, &word0));
            ThrowForError(_rdramReadAligned(wordAddr + 4, &word1));
            return (ushort) (word0 << 8 | word1 >> 24);
        }
        else
        {
            int byteShift = (int) ((2 - (addr & 3U)) * 8);
            uint word = 0;
            ThrowForError(_rdramReadAligned(wordAddr, &word));
            
            // 0x04030201
            return (ushort) (word >> byteShift);
        }
    }

    public static void RDRAM_Write16(uint addr, ushort val)
    {
        uint wordAddr = addr & ~3U;
        if ((addr & 3U) == 3)
        {
            ThrowForError(_rdramWriteAligned(wordAddr, (uint) (val >> 8), 0x000000FFU));
            ThrowForError(_rdramWriteAligned(wordAddr + 1, (uint) (val & 0xFF) << 24, 0xFF000000U));
        }
        else
        {
            int byteShift = (int) ((2 - (addr & 3U)) * 8);
            ThrowForError(_rdramWriteAligned(wordAddr, (uint) val << byteShift, 0xFFFFU << byteShift));
        }
    }

    public static uint RDRAM_Read32(uint addr)
    {
        uint wordAddr = addr & ~3U;
        if (addr == wordAddr)
        {
            uint word = 0;
            ThrowForError(_rdramReadAligned(wordAddr, &word));
            return word;
        }
        else
        {
            int byteShift = (int) ((addr & 3U) * 8);

            uint word0 = 0, word1 = 0;
            ThrowForError(_rdramReadAligned(wordAddr, &word0));
            ThrowForError(_rdramReadAligned(wordAddr + 4, &word1));

            return word0 << byteShift | word1 >> (24 - byteShift);
        }
    }

    public static void RDRAM_Write32(uint addr, uint val)
    {
        uint wordAddr = addr & ~3U;
        if (addr == wordAddr)
        {
            ThrowForError(_rdramWriteAligned(wordAddr, val));
        }
        else
        {
            int byteShift = (int) ((addr & 3U) * 8);
            
            ThrowForError(_rdramWriteAligned(wordAddr, val >> byteShift, 0xFFFFFFFFU >> byteShift));
            ThrowForError(_rdramWriteAligned(wordAddr, val << (24 - byteShift), 0xFFFFFFFFU << (24 - byteShift)));
        }
    }

    public static ulong RDRAM_Read64(uint addr)
    {
        return (ulong) RDRAM_Read32(addr) << 32 | RDRAM_Read32(addr + 4);
    }

    public static void RDRAM_Write64(uint addr, ulong val)
    {
        RDRAM_Write32(addr, (uint) (val >> 32));
        RDRAM_Write32(addr, (uint) (val & 0x00000000_FFFFFFFFU));
    }
}