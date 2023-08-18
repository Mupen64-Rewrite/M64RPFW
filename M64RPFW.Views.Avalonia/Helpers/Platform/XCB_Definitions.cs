using System.Runtime.InteropServices;

namespace M64RPFW.Views.Avalonia.Helpers.Platform;

internal static unsafe partial class XCB
{
    private const string LibX11_XCB = "libX11-xcb.so";
    private const string LibXCB = "libxcb.so";
    private const string LibXCB_XFixes = "libxcb-xfixes.so";
    
    /// <summary>
    /// Use only pointer to this type. <c>Connection*</c> represents a connection to the X server.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct Connection
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VoidCookie
    {
        public uint sequence;
    }

    /// <summary>
    /// Represents a rectangle, to be passed to various XCB methods.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rectangle
    {
        public short X;
        public short Y;
        public ushort Width;
        public ushort Height;
    }

    internal enum ConnectionError
    {
        None = 0,
        IOError = 1,
        ExtNotSupported = 2,
        MemInsufficient = 3,
        ReqLenExceed = 4,
        ParseError = 5,
        InvalidScreen = 6
    }

    

    internal enum ShapeSK
    {
        Bounding = 0,
        Clip = 1,
        Input = 2
    }
}