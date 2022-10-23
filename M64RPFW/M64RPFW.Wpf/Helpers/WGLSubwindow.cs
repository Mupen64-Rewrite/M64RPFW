using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using Microsoft.Win32.SafeHandles;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static Windows.Win32.Graphics.OpenGL.PFD_FLAGS;
using static Windows.Win32.Graphics.OpenGL.PFD_PIXEL_TYPE;
using static Windows.Win32.Graphics.OpenGL.PFD_LAYER_TYPE;

using OpenTK.Graphics.OpenGL4;

using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using OpenTK.Graphics.Wgl;

namespace M64RPFW.Wpf.Helpers
{
    internal class WGLSubwindow : IDisposable
    {
        private static readonly ConstPCWSTRString Win32ClassName = "m64rpfw-opengl-window";
        private static readonly ConstPCWSTRString BasicTitle = "M64RPFW OpenGL output";
        private static readonly PIXELFORMATDESCRIPTOR BasicPFD = new()
        {
            nSize = (ushort) Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
            iPixelType = PFD_TYPE_RGBA,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            cAuxBuffers = 0,
            iLayerType = PFD_MAIN_PLANE
        };

        static WGLSubwindow()
        {
            hInstance = (HINSTANCE) Marshal.GetHINSTANCE(typeof(WGLSubwindow).Module);
        }

        public unsafe WGLSubwindow(System.Windows.Window parent, Dictionary<GLAttribute, int> attrMap)
        {
            RegisterWndClass();
            HWND parentHwnd = (HWND) new WindowInteropHelper(parent).Handle;


            _isTempContext = true;
            _window = CreateWindowEx(
                0, Win32ClassName, BasicTitle, 
                WS_CHILD | WS_DISABLED, 
                0, 0, 100, 100, 
                parentHwnd, (HMENU) IntPtr.Zero, hInstance
            );
            var tmpDC = new ReleaseDCSafeHandle(_window, GetDC(_window));

            int truePixFmt = 0;
            try 
            {
                // Setup dummy pixel format
                int pixFmtID = ChoosePixelFormat(tmpDC, in BasicPFD);
                if (pixFmtID == 0)
                    throw new Win32Exception();
                if (SetPixelFormat(tmpDC, pixFmtID, in BasicPFD))
                    throw new Win32Exception();
                // Create dummy WGL context
                // This is needed because wglChoosePixelFormatARB is only available
                // via an OpenGL context
                var tmpWGL = wglCreateContext(tmpDC);
                if (!wglMakeCurrent(tmpDC, tmpWGL))
                    throw new Win32Exception();

                var attrs = WGLHelpers.GenAttributeLists(attrMap);
                var outFmt = new int[1];

                // Properly pick a pixel format
                GL.LoadBindings(new WGLBindingsContext());
                bool chooseRes = Wgl.Arb.ChoosePixelFormat(tmpDC.DangerousGetHandle(), attrs.pixFmt, null, 1, outFmt, out int nFmts);
                if (!chooseRes)
                    throw new Win32Exception();
                if (nFmts == 0)
                    throw new InvalidOperationException("INTERNAL: no suitable pixel formats found");
                truePixFmt = outFmt[0];
            }
            finally
            {
                tmpDC.Dispose();
                DestroyWindow(_window);
            }

            // We need a new window here because Windows only lets you
            // set the pixel format once
            _isTempContext = false;
            _window = CreateWindowEx(
                0, Win32ClassName, BasicTitle,
                WS_CHILD | WS_DISABLED,
                0, 0, 100, 100,
                parentHwnd, (HMENU) IntPtr.Zero, hInstance
            );

            _dc = GetDC(_window);
            if (_dc == IntPtr.Zero)
                throw new Win32Exception();

            // Install the shiny new pixel format
            PIXELFORMATDESCRIPTOR truePFD = new();
            if (DescribePixelFormat(_dc, (PFD_PIXEL_TYPE) truePixFmt, (uint) Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(), &truePFD) == 0)
                throw new Win32Exception();
            if (!SetPixelFormat(_dc, truePixFmt, &truePFD))
                throw new Win32Exception();

            // Reinitialize WGL
            _wgl = wglCreateContext(_dc);
        }

        private void RegisterWndClass()
        {
            WNDCLASSW wc = new WNDCLASSW();
            wc.lpfnWndProc = (hWnd, uMsg, wParam, lParam) =>
            {
                if (_isTempContext)
                    return DefWindowProc(hWnd, uMsg, wParam, lParam);
                else
                    return WindowProcedure(hWnd, uMsg, wParam, lParam);
            };
            wc.hInstance = (HINSTANCE) Marshal.GetHINSTANCE(typeof(WGLSubwindow).Module);
            wc.lpszClassName = Win32ClassName;
            wc.style = CS_OWNDC;

            RegisterClass(in wc);
        }

        private unsafe LRESULT WindowProcedure(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        {
            return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }

        public void Dispose()
        {
            if (wglGetCurrentContext() == _wgl)
                wglMakeCurrent(_dc, (HGLRC) IntPtr.Zero);
            wglDeleteContext(_wgl);

            ReleaseDC(_window, _dc);
            
            DestroyWindow(_window);
            _window = (HWND) IntPtr.Zero;
        }

        ~WGLSubwindow()
        {
            if ((IntPtr) _window != IntPtr.Zero)
                Dispose();
        }

        private static readonly HINSTANCE hInstance;
        private bool _isTempContext;
        private HDC _dc;
        private HGLRC _wgl;
        private HWND _window;
    }
}
