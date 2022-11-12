using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using OpenTK;

namespace M64PRR.Wpf.Interfaces;

public static unsafe class WGL
{
    #region Delegates

    private delegate BOOL D_wglChoosePixelFormatARB(
        HDC dc, int* piAttribIList, float* piAttribFList, uint nMaxFormats,
        int* piFormats, out uint nNumFormats);

    private static D_wglChoosePixelFormatARB? p_wglChoosePixelFormatARB;

    private delegate HGLRC D_wglCreateContextAttribsARB(
        HDC dc, HGLRC shareContext, int* attribList);

    private static D_wglCreateContextAttribsARB? p_wglCreateContextAttribsARB;

    private delegate BOOL D_wglGetPixelFormatAttribivARB(
        HDC hdc, int iPixelFormat, int iLayerPlane, uint nAttributes, int* piAttributes, int* piValues);

    private static D_wglGetPixelFormatAttribivARB? p_wglGetPixelFormatAttribivARB;

    private delegate BOOL D_wglSwapIntervalEXT(int interval);

    private static D_wglSwapIntervalEXT? p_wglSwapIntervalEXT;

    private delegate int D_wglGetSwapIntervalEXT();

    private static D_wglGetSwapIntervalEXT? p_wglGetSwapIntervalEXT;

    #endregion

    private static T? LoadFunction<T>(IBindingsContext context, string symbol) where T : Delegate
    {
        IntPtr addr = context.GetProcAddress(symbol);
        return addr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(addr);
    }

    internal static void LoadBindings(IBindingsContext context)
    {
        p_wglChoosePixelFormatARB = LoadFunction<D_wglChoosePixelFormatARB>(context, "wglChoosePixelFormatARB");
        p_wglCreateContextAttribsARB =
            LoadFunction<D_wglCreateContextAttribsARB>(context, "wglCreateContextAttribsARB");
        p_wglGetPixelFormatAttribivARB =
            LoadFunction<D_wglGetPixelFormatAttribivARB>(context, "wglGetPixelFormatAttribivARB");
        p_wglSwapIntervalEXT = LoadFunction<D_wglSwapIntervalEXT>(context, "wglSwapIntervalEXT");
        p_wglGetSwapIntervalEXT = LoadFunction<D_wglGetSwapIntervalEXT>(context, "wglGetSwapIntervalEXT");
    }

    internal static bool IsAvailable(string name)
    {
        return name switch
        {
            "wglChoosePixelFormatARB" => p_wglChoosePixelFormatARB is null,
            "wglCreateContextAttribsARB" => p_wglCreateContextAttribsARB is null,
            "wglGetPixelFormatAttribivARB" => p_wglGetPixelFormatAttribivARB is null,
            "wglSwapIntervalEXT" => p_wglSwapIntervalEXT is null,
            "wglGetSwapIntervalEXT" => p_wglGetSwapIntervalEXT is null,
            _ => false
        };
    }

    internal static BOOL wglChoosePixelFormatARB(
        HDC dc, int* piAttribIList, float* piAttribFList, uint nMaxFormats,
        int* piFormats, out uint nNumFormats)
    {
        if (p_wglChoosePixelFormatARB is null)
            throw new NotSupportedException("wglChoosePixelFormatARB is not available");
        return p_wglChoosePixelFormatARB(dc, piAttribIList, piAttribFList, nMaxFormats, piFormats, out nNumFormats);
    }

    internal static HGLRC wglCreateContextAttribsARB(
        HDC dc, HGLRC shareContext, int* attribList)
    {
        if (p_wglCreateContextAttribsARB is null)
            throw new NotSupportedException("wglCreateContextAttribsARB is not available");
        return p_wglCreateContextAttribsARB(dc, shareContext, attribList);
    }

    internal static BOOL wglGetPixelFormatAttribivARB(
        HDC hdc, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane, uint nAttributes, int* piAttributes, int* piValues)
    {
        if (p_wglGetPixelFormatAttribivARB is null)
            throw new NotSupportedException("wglGetPixelFormatAttribivARB is not available");
        return p_wglGetPixelFormatAttribivARB(hdc, iPixelFormat, (int) iLayerPlane, nAttributes, piAttributes,
            piValues);
    }

    internal static BOOL wglGetPixelFormatAttribivARB(
        HDC hdc, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane, int[] piAttributes, int[] piValues)
    {
        
        if (p_wglGetPixelFormatAttribivARB is null)
            throw new NotSupportedException("wglGetPixelFormatAttribivARB is not available");
        if (piAttributes.LongLength != piValues.LongLength)
            throw new ArgumentException(nameof(piAttributes));
        fixed (int* ppiAttributes = piAttributes, ppiValues = piValues)
            return p_wglGetPixelFormatAttribivARB(hdc, iPixelFormat, (int) iLayerPlane, (uint) piAttributes.LongLength,
                ppiAttributes, ppiValues);
    }

    internal static BOOL wglSwapIntervalEXT(int interval)
    {
        return p_wglSwapIntervalEXT(interval);
    }

    internal static int wglGetSwapIntervalEXT()
    {
        return p_wglGetSwapIntervalEXT();
    }
}