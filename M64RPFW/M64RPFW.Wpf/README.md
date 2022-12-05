# M64RPFW.Wpf

WPF/Win32-native components for Mupen64Plus Rerecording.

## Layout

- `Controls`: The actual WPF/Eto controls backing the emulator window.
- `Helpers`: General helper classes and Win32 window implementations.
- `Interfaces`: Library bindings that aren't handled by existing libraries.
    - `class WGL`: Bindings to WGL extensions.
    - `class Win32PInvoke`: Extra Win32 functions not available via [CsWin32](https://github.com/microsoft/CsWin32).