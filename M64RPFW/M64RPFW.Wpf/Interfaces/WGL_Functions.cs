using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;

namespace M64PRR.Wpf.Interfaces;

internal static partial class WGL
{
    #region Custom Function Loader

    public static void LoadFunctions()
    {
        var members = typeof(WGL).FindMembers(
            MemberTypes.Field,
            BindingFlags.Static | BindingFlags.NonPublic,
            null, null);

        foreach (var m in members)
        {
            FieldInfo field = (FieldInfo) m;
            Type t = field.FieldType;
            if (t.Name.StartsWith("pfn_") && t.DeclaringType == typeof(WGL))
            {
                string fnName = t.Name.Substring(4);
                IntPtr fnPtr = PInvoke.wglGetProcAddress(fnName);
                field.SetValue(null, fnPtr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(fnPtr, t));
            }
        }

        IsLoaded = true;
    }

    public static bool IsLoaded { get; private set; } = false;

    #endregion

    #region wglGetExtensionsStringARB

    private static pfn_wglGetExtensionsStringARB? _wglGetExtensionsStringARB = null;

    public static unsafe string GetExtensionsStringARB(HDC hdc)
    {
        if (_wglGetExtensionsStringARB is null)
            throw new InvalidOperationException("wglGetExtensionsStringARB is not loaded");
        char* cstr = _wglGetExtensionsStringARB(hdc);
        string? str = Marshal.PtrToStringAnsi((IntPtr) cstr);

        return str ?? throw new NullReferenceException("Extensions string is null");
    }

    public static string GetExtensionsStringARB(SafeHandle hdc)
    {
        if (_wglGetExtensionsStringARB is null)
            throw new InvalidOperationException("wglGetExtensionsStringARB is not loaded");
        bool hdcRef = false;
        try
        {
            hdc.DangerousAddRef(ref hdcRef);
            return GetExtensionsStringARB((HDC) hdc.DangerousGetHandle());
        }
        finally
        {
            if (hdcRef)
                hdc.DangerousRelease();
        }
    }

    #endregion

    #region wglChoosePixelFormatARB

    private static pfn_wglChoosePixelFormatARB? _wglChoosePixelFormatARB = null;

    public static unsafe BOOL ChoosePixelFormatARB(HDC hDC, int* piAttribIList, float* piAttribFList, uint nMaxFormats,
        int* piFormats, uint* nNumFormats)
    {
        if (_wglChoosePixelFormatARB is null)
            throw new InvalidOperationException("wglChoosePixelFormatARB is not loaded");
        return _wglChoosePixelFormatARB(hDC, piAttribIList, piAttribFList, nMaxFormats, piFormats, nNumFormats);
    }

    public static unsafe BOOL ChoosePixelFormatARB(SafeHandle hDC, int[]? piAttribIList, float[]? piAttribFList,
        int[] piFormats, out uint nNumFormats)
    {
        if (_wglChoosePixelFormatARB is null)
            throw new InvalidOperationException("wglChoosePixelFormatARB is not loaded");

        bool hDC_Ref = false;
        try
        {
            hDC.DangerousAddRef(ref hDC_Ref);

            fixed (int* ppiAttribIList = piAttribIList, ppiFormats = piFormats)
            fixed (uint* pnNumFormats = &nNumFormats)
            fixed (float* ppiAttribFList = piAttribFList)
            {
                return _wglChoosePixelFormatARB((HDC) hDC.DangerousGetHandle(), ppiAttribIList, ppiAttribFList,
                    (uint) piFormats.Length, ppiFormats, pnNumFormats);
            }
        }
        finally
        {
            if (hDC_Ref)
            {
                hDC.DangerousRelease();
            }
        }
    }

    #endregion

    #region wglGetPixelFormatAttribivARB

    private static pfn_wglGetPixelFormatAttribivARB? _wglGetPixelFormatAttribivARB = null;

    public static unsafe BOOL GetPixelFormatAttribs(HDC hDC, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane,
        uint nAttributes, int* piAttributes, int* piValues)
    {
        if (_wglGetPixelFormatAttribivARB is null)
            throw new InvalidOperationException("wglGetPixelFormatAttribivARB is not loaded");

        return _wglGetPixelFormatAttribivARB(hDC, iPixelFormat, iLayerPlane, nAttributes, piAttributes, piValues);
    }

    public static unsafe BOOL GetPixelFormatAttribs(SafeHandle hDC, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane,
        int[] piAttributes, int[] piValues)
    {
        if (_wglGetPixelFormatAttribivARB is null)
            throw new InvalidOperationException("wglGetPixelFormatAttribivARB is not loaded");
        if (piValues.Length < piAttributes.Length)
            throw new ArgumentException("piValues.Length must be >= piAttributes.Length.", nameof(piValues));

        bool hDC_Ref = false;
        try
        {
            hDC.DangerousAddRef(ref hDC_Ref);
            fixed (int* ppiAttributes = piAttributes, ppiValues = piValues)
            {
                return _wglGetPixelFormatAttribivARB((HDC) hDC.DangerousGetHandle(), iPixelFormat, iLayerPlane,
                    (uint) piAttributes.Length, ppiAttributes, ppiValues);
            }
        }
        finally
        {
            if (hDC_Ref)
            {
                hDC.DangerousRelease();
            }
        }
    }

    #endregion

    #region wglGetPixelFormatAttribfvARB

    private static pfn_wglGetPixelFormatAttribfvARB? _wglGetPixelFormatAttribfvARB = null;

    public static unsafe BOOL GetPixelFormatAttribs(HDC hDC, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane,
        uint nAttributes, int* piAttributes, float* piValues)
    {
        if (_wglGetPixelFormatAttribfvARB is null)
            throw new InvalidOperationException("wglGetPixelFormatAttribivARB is not loaded");

        return _wglGetPixelFormatAttribfvARB(hDC, iPixelFormat, iLayerPlane, nAttributes, piAttributes, piValues);
    }

    public static unsafe BOOL GetPixelFormatAttribs(SafeHandle hDC, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane,
        int[] piAttributes, float[] piValues)
    {
        if (_wglGetPixelFormatAttribfvARB is null)
            throw new InvalidOperationException("wglGetPixelFormatAttribivARB is not loaded");
        if (piValues.Length < piAttributes.Length)
            throw new ArgumentException("piValues.Length must be >= piAttributes.Length.", nameof(piValues));

        bool hDC_Ref = false;
        try
        {
            hDC.DangerousAddRef(ref hDC_Ref);
            fixed (int* ppiAttributes = piAttributes)
            fixed (float* ppiValues = piValues)
            {
                return _wglGetPixelFormatAttribfvARB((HDC) hDC.DangerousGetHandle(), iPixelFormat, iLayerPlane,
                    (uint) piAttributes.Length, ppiAttributes, ppiValues);
            }
        }
        finally
        {
            if (hDC_Ref)
            {
                hDC.DangerousRelease();
            }
        }
    }

    #endregion

    #region wglCreateContextAttribsARB

    private static pfn_wglCreateContextAttribsARB? _wglCreateContextAttribsARB = null;

    public static unsafe HGLRC CreateContextAttribsARB(HDC hDC, HGLRC hShareContext, int* attribList)
    {
        if (_wglCreateContextAttribsARB is null)
            throw new InvalidOperationException("wglCreateContextAttribsARB is not loaded");
        return _wglCreateContextAttribsARB(hDC, hShareContext, attribList);
    }

    public static unsafe wglDeleteContextSafeHandle CreateContextAttribsARB(SafeHandle hDC, SafeHandle? hShareContext, int[] attribList)
    {
        if (_wglCreateContextAttribsARB is null)
            throw new InvalidOperationException("wglCreateContextAttribsARB is not loaded");
        
        bool hDCRef = false, hShareContextRef = false;
        try
        {
            hDC.DangerousAddRef(ref hDCRef);
            if (hShareContext != null)
                hShareContext.DangerousAddRef(ref hShareContextRef);

            HGLRC hShareContextRaw = hShareContext == null ? HGLRC.Null : (HGLRC) hShareContext.DangerousGetHandle();
            
            fixed (int* pAttribList = attribList)
            {
                return new wglDeleteContextSafeHandle(_wglCreateContextAttribsARB((HDC) hDC.DangerousGetHandle(), hShareContextRaw, pAttribList));
            }
        }
        finally
        {
            if (hDCRef)
                hDC.DangerousRelease();
            if (hShareContextRef)
                hShareContext.DangerousRelease();
        }
    }

    #endregion

    #region wgl(Get)SwapIntervalEXT

    private static pfn_wglSwapIntervalEXT? _wglSwapIntervalEXT = null;
    private static pfn_wglGetSwapIntervalEXT? _wglGetSwapIntervalEXT = null;

    public static BOOL SwapIntervalEXT(int interval)
    {
        if (_wglSwapIntervalEXT is null)
            throw new InvalidOperationException("wglSwapIntervalEXT() is not loaded");
        return _wglSwapIntervalEXT(interval);
    }

    public static int GetSwapIntervalEXT()
    {
        if (_wglGetSwapIntervalEXT is null)
            throw new InvalidOperationException("wglGetSwapIntervalEXT() is not loaded");
        return _wglGetSwapIntervalEXT();
    }
    
    #endregion
}