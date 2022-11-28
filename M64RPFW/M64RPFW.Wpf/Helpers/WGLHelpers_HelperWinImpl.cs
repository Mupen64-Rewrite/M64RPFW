using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using Windows.Win32.UI.WindowsAndMessaging;
using M64PRR.Wpf.Interfaces;
using OpenTK;
using OpenTK.Graphics.Wgl;
using static Windows.Win32.Graphics.OpenGL.PFD_FLAGS;
using static Windows.Win32.Graphics.OpenGL.PFD_PIXEL_TYPE;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.PEEK_MESSAGE_REMOVE_TYPE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_EX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static M64PRR.Wpf.Interfaces.Win32PInvoke;

namespace M64RPFW.Wpf.Helpers;

internal partial class WGLHelpers
{
    private const string WINDOW_CLASS = "RPFW-Helper";

    private static readonly DestroyWindowSafeHandle helperWindow;
    private static readonly ReleaseDCSafeHandle helperDC;
    private static readonly wglDeleteContextSafeHandle helperRC;

    private static LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case WM_DESTROY:
            {
                PostQuitMessage(0);
                return (LRESULT) 0;
            }
            default:
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }
    
    private static ImmutableHashSet<string> extensionList;

    static unsafe WGLHelpers()
    {
        // Adapted and reduced from GLFW:
        // https://github.com/glfw/glfw/blob/d299d9f78857e921b66bdab42c7ea27fe2e31810/src/win32_init.c#L373-L433
        // https://github.com/glfw/glfw/blob/dd8a678a66f1967372e5a5e3deac41ebf65ee127/src/wgl_context.c#L390-L506

        #region Init the helper window

        fixed (char* pWindowClass = WINDOW_CLASS)
        {
            WNDCLASSEXW wndClass = new()
            {
                cbSize = (uint) Marshal.SizeOf<WNDCLASSEXW>(),
                style = CS_OWNDC | CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = WindowProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = CurrentHInstanceRaw,
                hIcon = HICON.Null,
                hCursor = LoadCursor(HINSTANCE.Null, IDC_ARROW),
                hbrBackground = HBRUSH.Null,
                lpszMenuName = null,
                lpszClassName = pWindowClass,
                hIconSm = HICON.Null
            };

            RegisterClassEx(in wndClass);
        }

        helperWindow = CreateWindowEx_SafeHandle2(
            WS_EX_OVERLAPPEDWINDOW, WINDOW_CLASS, "M64RPFW Helper", 
            WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
            0, 0, 1, 1,HWND.Null, null, null);

        while (PeekMessage(out var msg, helperWindow, 0, 0, PM_REMOVE))
        {
            TranslateMessage(msg);
            DispatchMessage(msg);
        }

        #endregion

        #region Init WGL
        
        // Several WGL functions require an active OpenGL context, so we use the legacy pixel format
        // system to do that
        
        PIXELFORMATDESCRIPTOR pfd = default;
        pfd.nSize = (ushort) Marshal.SizeOf<PIXELFORMATDESCRIPTOR>();
        pfd.nVersion = 1;
        pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
        pfd.iPixelType = PFD_TYPE_RGBA;
        pfd.cColorBits = 24;

        helperDC = GetDC_SafeHandle2(helperWindow);

        int legacyPixelFmt = ChoosePixelFormat(helperDC, in pfd);
        if (legacyPixelFmt == 0)
            throw new Win32Exception();

        if (!SetPixelFormat(helperDC, legacyPixelFmt, in pfd))
            throw new Win32Exception();

        helperRC = wglCreateContext(helperDC);

        if (!wglMakeCurrent(helperDC, helperRC))
            throw new Win32Exception();
        
        // Load bindings
        WGL.LoadFunctions();

        #endregion

        #region Check extensions

        extensionList = WGL.GetExtensionsStringARB(helperDC).Split(" ").ToImmutableHashSet();
        
        // These extensions are necessary.
        Trace.Assert(extensionList.Contains("WGL_ARB_create_context"));
        Trace.Assert(extensionList.Contains("WGL_ARB_create_context_profile"));
        Trace.Assert(extensionList.Contains("WGL_ARB_pixel_format"));

        #endregion
    }
}