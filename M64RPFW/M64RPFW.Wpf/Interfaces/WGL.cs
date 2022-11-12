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

    private static D_wglChoosePixelFormatARB p_wglChoosePixelFormatARB;
    
    private delegate HGLRC D_wglCreateContextAttribsARB(
        HDC dc, HGLRC shareContext, int* attribList);

    private static D_wglCreateContextAttribsARB p_wglCreateContextAttribsARB;
    
    private delegate BOOL D_wglGetPixelFormatAttribivARB(
        HDC hdc, int iPixelFormat, int iLayerPlane, uint nAttributes, int* piAttributes, int* piValues);

    private static D_wglGetPixelFormatAttribivARB p_wglGetPixelFormatAttribivARB;
    
    private delegate BOOL D_wglSwapIntervalEXT(int interval);

    private static D_wglSwapIntervalEXT p_wglSwapIntervalEXT;

    private delegate int D_wglGetSwapIntervalEXT();

    private static D_wglGetSwapIntervalEXT p_wglGetSwapIntervalEXT;
    #endregion

    internal static void LoadBindings(IBindingsContext context)
    {
        p_wglChoosePixelFormatARB = Marshal.GetDelegateForFunctionPointer<D_wglChoosePixelFormatARB>(
            context.GetProcAddress("wglChoosePixelFormatARB"));

        p_wglCreateContextAttribsARB = Marshal.GetDelegateForFunctionPointer<D_wglCreateContextAttribsARB>(
            context.GetProcAddress("wglCreateContextAttribsARB"));

        p_wglGetPixelFormatAttribivARB = Marshal.GetDelegateForFunctionPointer<D_wglGetPixelFormatAttribivARB>(
            context.GetProcAddress("wglGetPixelFormatAttribivARB"));
        
        p_wglSwapIntervalEXT = Marshal.GetDelegateForFunctionPointer<D_wglSwapIntervalEXT>(
            context.GetProcAddress("wglSwapIntervalEXT"));

        p_wglGetSwapIntervalEXT = Marshal.GetDelegateForFunctionPointer<D_wglGetSwapIntervalEXT>(
            context.GetProcAddress("wglGetSwapIntervalEXT"));
    }

    internal static BOOL wglChoosePixelFormatARB(
        HDC dc, int* piAttribIList, float* piAttribFList, uint nMaxFormats,
        int* piFormats, out uint nNumFormats)
    {
        return p_wglChoosePixelFormatARB(dc, piAttribIList, piAttribFList, nMaxFormats, piFormats, out nNumFormats);
    }

    internal static HGLRC wglCreateContextAttribsARB(
        HDC dc, HGLRC shareContext, int* attribList)
    {
        return p_wglCreateContextAttribsARB(dc, shareContext, attribList);
    }

    internal static BOOL wglGetPixelFormatAttribivARB(
        HDC hdc, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane, uint nAttributes, int* piAttributes, int* piValues)
    {
        return p_wglGetPixelFormatAttribivARB(hdc, iPixelFormat, (int) iLayerPlane, nAttributes, piAttributes, piValues);
    }
    
    internal static BOOL wglGetPixelFormatAttribivARB(
        HDC hdc, int iPixelFormat, PFD_LAYER_TYPE iLayerPlane, int[] piAttributes, int[] piValues)
    {
        if (piAttributes.LongLength != piValues.LongLength)
            throw new ArgumentException(nameof(piAttributes));
        fixed (int* ppiAttributes = piAttributes, ppiValues = piValues)
            return p_wglGetPixelFormatAttribivARB(hdc, iPixelFormat, (int) iLayerPlane, (uint) piAttributes.LongLength, ppiAttributes, ppiValues);
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