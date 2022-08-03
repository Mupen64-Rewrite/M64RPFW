# M64PRR.Gtk
GTK#/Linux-native components for Mupen64Plus Rerecording.

## Layout
- `Controls`: The actual GTK#/Eto controls backing the emulator window.
- `Helpers`: General helper classes and X11/WL window implementations.
- `Interfaces`: Library bindings that aren't handled by existing libraries.
  - `Wayland`: XML protocol bindings for WaylandSharp.
  - `class LibGdk`: Bindings to X11/WL-specific functions in GDK.
  - `class LibGLib`: Implementation of GType macros in C#
  - `class LibWlEGL`: Bindings to `libwayland-egl.so`, the glue between Wayland and EGL.