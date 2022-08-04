using System;
using System.Runtime.InteropServices;

namespace M64PRR.Models.Helpers;

public static class NativeLibHelper
{
    public static T GetFunction<T>(IntPtr lib, string name)
    {
        return Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(lib, name));
    }
}