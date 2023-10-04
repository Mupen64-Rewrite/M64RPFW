using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public unsafe partial class Mupen64Plus
{
    #region Encoder delegates
    
    // Encoder callbacks
    // =================
    private delegate void SampleCallback(void* sample, nint sampleLen);

    private delegate void RateChangedCallback(uint rate);
    
    // Encoder functions
    // =================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    [RuntimeDllImport]
    private delegate bool DEncoder_IsActive();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DEncoder_Start(byte* path, byte* format);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DEncoder_Stop([MarshalAs(UnmanagedType.I1)] bool discard);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DEncoder_SetSampleCallback(SampleCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DEncoder_SetRateChangedCallback(RateChangedCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate uint DEncoder_GetSampleRate();

    #endregion

    private static DEncoder_IsActive _encoderIsActive;
    private static DEncoder_Start _encoderStart;
    private static DEncoder_Stop _encoderStop;

    private static DEncoder_SetSampleCallback _encoderSetSampleCallback;
    private static DEncoder_SetRateChangedCallback _encoderSetRateChangedCallback;
    private static DEncoder_GetSampleRate _encoderGetSampleRate;

    private static void ResolveEncoderFunctions()
    {
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderIsActive);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderStart);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderStop);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderSetSampleCallback);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderSetRateChangedCallback);
        NativeLibHelper.ResolveDelegate(_libHandle, out _encoderGetSampleRate);
    }
    
    
}