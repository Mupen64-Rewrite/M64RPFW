using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static M64PRR.Wpf.Interfaces.Win32PInvoke;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow
{
    private const string WINDOW_CLASS = "RPFW-Main";
    
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
    
    static unsafe Win32SubWindow()
    {
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
    }
}