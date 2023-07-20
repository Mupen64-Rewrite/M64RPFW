using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public unsafe partial class Mupen64Plus
{
    #region RDRAM delegates
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DRDRAM_ReadAligned(uint addr, uint* value);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DRDRAM_WriteAligned(uint addr, uint value, uint mask = 0xFFFFFFFFU);

    #endregion

    private static DRDRAM_ReadAligned _rdramReadAligned;
    private static DRDRAM_WriteAligned _rdramWriteAligned;

    private static void ResolveRdramFunctions()
    {
        NativeLibHelper.ResolveDelegate(_libHandle, out _rdramReadAligned);
        NativeLibHelper.ResolveDelegate(_libHandle, out _rdramWriteAligned);
    }
}