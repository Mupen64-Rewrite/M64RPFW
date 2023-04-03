using System.Runtime.InteropServices;

namespace M64RPFW.Models.Types;

public static partial class Mupen64PlusTypes
{
    [Flags]
    public enum ButtonMask : ushort
    {
        A = 0x8000,
        B = 0x4000,
        Z = 0x2000,
        Start = 0x1000,
        DUp = 0x0800,
        DDown = 0x0400,
        DLeft = 0x0200,
        DRight = 0x0100,
        Unknown1 = 0x0080,
        Unknown2 = 0x0040,
        L = 0x0020,
        R = 0x0010,
        CUp = 0x0008,
        CDown = 0x0004,
        CLeft = 0x0002,
        CRight = 0x0001,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Buttons
    {
        [FieldOffset(0)]
        public ButtonMask BtnMask;
        [FieldOffset(2)]
        public sbyte JoyX;
        [FieldOffset(3)]
        public sbyte JoyY;
    }

    public enum VCRStartType
    {
        FromSnapshot = 1,
        FromReset = 2,
        FromEEPROM = 4,
    }

    public enum VCRParam
    {
        State = 0,
        Readonly
    }
}