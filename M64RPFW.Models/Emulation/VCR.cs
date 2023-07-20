using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using static M64RPFW.Models.Helpers.NativeLibHelper;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    #region VCR callbacks

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool VCRMsgFunc(MessageLevel lvl, string msg);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void VCRStateCallback(VCRParam param, int value);

    #endregion

    #region VCR delegates

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void DVCR_SetErrorCallback(VCRMsgFunc callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void DVCR_SetStateCallback(VCRStateCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate uint DVCR_GetCurFrame();

    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void DVCR_StopMovie([MarshalAs(UnmanagedType.Bool)] bool restart);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void DVCR_SetKeys(Buttons keys, uint port);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void DVCR_GetKeys(out Buttons keys, uint port);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool DVCR_IsPlaying();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool DVCR_IsReadOnly();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool DVCR_SetReadOnly([MarshalAs(UnmanagedType.Bool)] bool state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DVCR_StartRecording(string path, string author, string desc, VCRStartType startType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DVCR_StartMovie(string path);

    #endregion

    private static void ResolveVcrFunctions()
    {
        ResolveDelegate(_libHandle, out _vcrSetErrorCallback);
        ResolveDelegate(_libHandle, out _vcrSetStateCallback);
        ResolveDelegate(_libHandle, out _vcrGetCurFrame);
        ResolveDelegate(_libHandle, out _vcrStopMovie);
        ResolveDelegate(_libHandle, out _vcrSetKeys);
        ResolveDelegate(_libHandle, out _vcrGetKeys);
        ResolveDelegate(_libHandle, out _vcrIsPlaying);
        ResolveDelegate(_libHandle, out _vcrIsReadOnly);
        ResolveDelegate(_libHandle, out _vcrSetReadOnly);
        ResolveDelegate(_libHandle, out _vcrStartRecording);
        ResolveDelegate(_libHandle, out _vcrStartMovie);
    }

    private static DVCR_SetErrorCallback _vcrSetErrorCallback;
    private static DVCR_SetStateCallback _vcrSetStateCallback;
    private static DVCR_GetCurFrame _vcrGetCurFrame;
    private static DVCR_StopMovie _vcrStopMovie;
    private static DVCR_SetKeys _vcrSetKeys;
    private static DVCR_GetKeys _vcrGetKeys;
    private static DVCR_IsPlaying _vcrIsPlaying;
    private static DVCR_IsReadOnly _vcrIsReadOnly;
    private static DVCR_SetReadOnly _vcrSetReadOnly;
    private static DVCR_StartRecording _vcrStartRecording;
    private static DVCR_StartMovie _vcrStartMovie;


}