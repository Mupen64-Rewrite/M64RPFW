using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public unsafe partial class Mupen64Plus
{
    #region Encoder delegates

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool DEncoder_IsActive();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Mupen64PlusTypes.Error DEncoder_Start(byte* path, Mupen64PlusTypes.EncoderFormat format);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Mupen64PlusTypes.Error DEncoder_Stop([MarshalAs(UnmanagedType.I1)] bool discard);

    #endregion

    private static DEncoder_IsActive _encoderIsActive;
    private static DEncoder_Start _encoderStart;
    private static DEncoder_Stop _encoderStop;

    private static void ResolveEncoderFunctions()
    {
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderIsActive);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderStart);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderStop);
    }
}