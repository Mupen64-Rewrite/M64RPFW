using System.Runtime.InteropServices;

namespace M64RPFW.Models.Types;

public static partial class Mupen64PlusTypes
{
    [Flags]
    public enum ButtonMask : ushort
    {
        DRight = (1 << 0),
        DLeft = (1 << 1),
        DDown = (1 << 2),
        DUp = (1 << 3),
        Start = (1 << 4),
        Z = (1 << 5),
        B = (1 << 6),
        A = (1 << 7),
        CRight = (1 << 8),
        CLeft = (1 << 9),
        CDown = (1 << 10),
        CUp = (1 << 11),
        R = (1 << 12),
        L = (1 << 13),
        Reserved1 = (1 << 14),
        Reserved2 = (1 << 15),
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
        Snapshot = 1,
        Reset = 2,
        // ReSharper disable once InconsistentNaming
        EEPROM = 4,
    }

    public enum VCRParam
    {
        State = 0,
        ReadOnly
    }
}