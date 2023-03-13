namespace M64RPFW.Gtk.Interfaces;

/// <summary>
/// EGL constants and functions not provided by OpenTK.
/// </summary>
public class LibEGL
{
    public const int CONTEXT_MAJOR_VERSION_KHR = 0x3098;
    public const int CONTEXT_MINOR_VERSION_KHR = 0x30FB;
    public const int CONTEXT_OPENGL_PROFILE_MASK_KHR = 0x30FD;

    public const int CONTEXT_OPENGL_CORE_PROFILE_BIT_KHR = 0x00000001;
    public const int CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR = 0x00000002;
}