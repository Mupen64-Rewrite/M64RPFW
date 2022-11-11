using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using OpenTK;

namespace M64PRR.Wpf.Interfaces;

public static unsafe class WGLBindings
{
    #region Delegates

    private delegate BOOL D_wglChoosePixelFormatARB(
        HDC dc, int* piAttribIList, float* piAttribFList, uint nMaxFormats,
        int* piFormats, out uint nNumFormats);

    private static D_wglChoosePixelFormatARB p_wglChoosePixelFormatARB;
    
    private delegate HGLRC D_wglCreateContextAttribsARB(
        HDC dc, HGLRC shareContext, int* attribList);

    private static D_wglCreateContextAttribsARB p_wglCreateContextAttribsARB;

    #endregion

    internal static void LoadBindings(IBindingsContext context)
    {
        p_wglChoosePixelFormatARB = Marshal.GetDelegateForFunctionPointer<D_wglChoosePixelFormatARB>(
            context.GetProcAddress("wglChoosePixelFormatARB"));

        p_wglCreateContextAttribsARB = Marshal.GetDelegateForFunctionPointer<D_wglCreateContextAttribsARB>(
            context.GetProcAddress("wglCreateContextAttribsARB"));
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
}