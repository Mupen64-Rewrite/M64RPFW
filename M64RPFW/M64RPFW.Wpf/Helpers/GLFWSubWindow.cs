using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Drawing;

using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.PInvoke;

using OpenTK.Graphics.OpenGL4;

using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using static M64RPFW.Wpf.Helpers.Win32Extras;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace M64RPFW.Wpf.Helpers
{
    internal unsafe class GLFWSubWindow : IDisposable
    {
        public GLFWSubWindow(System.Windows.Window parent, Size size, IDictionary<GLAttribute, int> attrs)
        {
            // Create window using GLFW
            // ========================
            
            GLFW.Init();
            GLFW.WindowHint(WindowHintBool.Decorated, false);
            GLFW.WindowHint(WindowHintBool.Visible, false);

            GLFWHelpers.InterpretAttributes(attrs);

            int realWidth = size.Width;
            int realHeight = size.Height;

            realWidth = (realWidth < 640) ? 640 : realWidth;
            realHeight = (realHeight < 480) ? 480 : realHeight;

            _glfwWindow = GLFW.CreateWindow(realWidth, realHeight, "", null, null);

            if (_glfwWindow == null)
            {
                GLFW.GetError(out string msg);
                throw new ApplicationException($"GLFW error: {msg}");
            }

            if (attrs.TryGetValue(GLAttribute.SwapControl, out int attrValue))
            {
                GLFW.SwapInterval(attrValue);
            }

            GL.LoadBindings(new GLFWBindingsContext());

            // Use raw Win32 to turn it into a child window
            // ============================================

            int err;

            _glfwHwnd = (HWND) GLFW.GetWin32Window(_glfwWindow);
            HWND parentHwnd = (HWND) new WindowInteropHelper(parent).Handle;

            // change parent to main window
            if (SetParent(_glfwHwnd, parentHwnd) == IntPtr.Zero)
                throw new Win32Exception();

            // change style to "child window" and prevent it from
            // getting input
            Marshal.SetLastSystemError(0);
            WINDOW_STYLE prevStyle = (WINDOW_STYLE) GetWindowLong(_glfwHwnd, GWL_STYLE);
            err = Marshal.GetLastSystemError();
            if (err != 0)
                throw new Win32Exception(err);

            prevStyle &= ~(WS_POPUP | WS_OVERLAPPED);
            prevStyle |= WS_CHILD | WS_DISABLED;

            Marshal.SetLastSystemError(0);
            nint result = SetWindowLongPtr(_glfwHwnd, GWL_STYLE, (nint) prevStyle);
            err = Marshal.GetLastSystemError();
            if (result == 0 && err != 0)
                throw new Win32Exception(err);

            // *now* display it
            ShowWindow(_glfwHwnd, SW_SHOWNOACTIVATE);
        }

        public void MakeCurrent()
        {
            GLFW.MakeContextCurrent(_glfwWindow);
        }

        public void SwapBuffers()
        {
            GLFW.SwapBuffers(_glfwWindow);
            GLFW.PollEvents();
        }

        public void SetPosition(Point p)
        {
            if (!GetWindowRect(_glfwHwnd, out RECT bounds))
                throw new Win32Exception();
            if (!MoveWindow(_glfwHwnd, p.X, p.Y, bounds.Width, bounds.Height, false)) 
                throw new Win32Exception();
        }

        public IntPtr GetProcAddress(string symbol)
        {
            return GLFW.GetProcAddress(symbol);
        }

        public int GetAttribute(GLAttribute attr)
        {
            return GLFWHelpers.QueryAttribute(attr, _glfwWindow);
        }

        public void ResizeWindow(Size s)
        {
            if (!GetWindowRect(_glfwHwnd, out RECT bounds))
                throw new Win32Exception();
            if (!MoveWindow(_glfwHwnd, bounds.X, bounds.Y, s.Width, s.Height, false))
                throw new Win32Exception();
        }

        public void Dispose()
        {
            EnableWindow(_glfwHwnd, true);

            GLFW.DestroyWindow(_glfwWindow);
            _glfwWindow = null;
        }

        public void SetVisible(bool visible)
        {
            SHOW_WINDOW_CMD cmd = visible ? SW_SHOWNOACTIVATE : SW_HIDE;
            ShowWindow(_glfwHwnd, cmd);
        }

        ~GLFWSubWindow()
        {
            EnableWindow(_glfwHwnd, true);

            if (_glfwWindow != null)
                GLFW.DestroyWindow(_glfwWindow);
        }


        private Window* _glfwWindow;
        private HWND _glfwHwnd;
    }
}
