// This code was taken from OpenTK with a few minor tweaks, and so I include
// their license here as credit to them. Thank you, fellow open source devs.
// ============================================================================
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// ============================================================================

using System;
using System.Runtime.InteropServices;

namespace M64RPFW.Gtk.Interfaces;

using EGLNativeDisplayType = IntPtr;
using EGLNativeWindowType = IntPtr;
using EGLNativePixmapType = IntPtr;
using EGLConfig = IntPtr;
using EGLContext = IntPtr;
using EGLDisplay = IntPtr;
using EGLSurface = IntPtr;
using EGLClientBuffer = IntPtr;

public enum RenderApi
{
    ES = LibEGL.OPENGL_ES_API,
    GL = LibEGL.OPENGL_API,
    VG = LibEGL.OPENVG_API
}

[Flags]
public enum RenderableFlags
{
    ES = LibEGL.OPENGL_ES_BIT,
    ES2 = LibEGL.OPENGL_ES2_BIT,
    ES3 = LibEGL.OPENGL_ES3_BIT,
    GL = LibEGL.OPENGL_BIT,
    VG = LibEGL.OPENVG_BIT,
}

public enum ErrorCode
{
    SUCCESS = 12288,
    NOT_INITIALIZED = 12289,
    BAD_ACCESS = 12290,
    BAD_ALLOC = 12291,
    BAD_ATTRIBUTE = 12292,
    BAD_CONFIG = 12293,
    BAD_CONTEXT = 12294,
    BAD_CURRENT_SURFACE = 12295,
    BAD_DISPLAY = 12296,
    BAD_MATCH = 12297,
    BAD_NATIVE_PIXMAP = 12298,
    BAD_NATIVE_WINDOW = 12299,
    BAD_PARAMETER = 12300,
    BAD_SURFACE = 12301,
    CONTEXT_LOST = 12302,
}

public enum SurfaceType
{
    PBUFFER_BIT = 0x0001,
    PIXMAP_BIT = 0x0002,
    WINDOW_BIT = 0x0004,
    VG_COLORSPACE_LINEAR_BIT = 0x0020,
    VG_ALPHA_FORMAT_PRE_BIT = 0x0040,
    MULTISAMPLE_RESOLVE_BOX_BIT = 0x0200,
    SWAP_BEHAVIOR_PRESERVED_BIT = 0x0400,
}

/// <summary>
/// EGL bindings stolen from the OpenTK 3.x branch.
/// This is enough for now until OpenTK 4.x gets EGL.
/// </summary>
public static partial class LibEGL
{
    private const string LibName = "libEGL.so";

    public const int VERSION_1_0 = 1;
    public const int VERSION_1_1 = 1;
    public const int VERSION_1_2 = 1;
    public const int VERSION_1_3 = 1;
    public const int VERSION_1_4 = 1;
    public const int FALSE = 0;
    public const int TRUE = 1;
    public const int DONT_CARE = -1;
    public const int CONTEXT_LOST = 12302;
    public const int BUFFER_SIZE = 12320;
    public const int ALPHA_SIZE = 12321;
    public const int BLUE_SIZE = 12322;
    public const int GREEN_SIZE = 12323;
    public const int RED_SIZE = 12324;
    public const int DEPTH_SIZE = 12325;
    public const int STENCIL_SIZE = 12326;
    public const int CONFIG_CAVEAT = 12327;
    public const int CONFIG_ID = 12328;
    public const int LEVEL = 12329;
    public const int MAX_PBUFFER_HEIGHT = 12330;
    public const int MAX_PBUFFER_PIXELS = 12331;
    public const int MAX_PBUFFER_WIDTH = 12332;
    public const int NATIVE_RENDERABLE = 12333;
    public const int NATIVE_VISUAL_ID = 12334;
    public const int NATIVE_VISUAL_TYPE = 12335;
    public const int PRESERVED_RESOURCES = 12336;
    public const int SAMPLES = 12337;
    public const int SAMPLE_BUFFERS = 12338;
    public const int SURFACE_TYPE = 12339;
    public const int TRANSPARENT_TYPE = 12340;
    public const int TRANSPARENT_BLUE_VALUE = 12341;
    public const int TRANSPARENT_GREEN_VALUE = 12342;
    public const int TRANSPARENT_RED_VALUE = 12343;
    public const int NONE = 12344;
    public const int BIND_TO_TEXTURE_RGB = 12345;
    public const int BIND_TO_TEXTURE_RGBA = 12346;
    public const int MIN_SWAP_INTERVAL = 12347;
    public const int MAX_SWAP_INTERVAL = 12348;
    public const int LUMINANCE_SIZE = 12349;
    public const int ALPHA_MASK_SIZE = 12350;
    public const int COLOR_BUFFER_TYPE = 12351;
    public const int RENDERABLE_TYPE = 12352;
    public const int MATCH_NATIVE_PIXMAP = 12353;
    public const int CONFORMANT = 12354;
    public const int SLOW_CONFIG = 12368;
    public const int NON_CONFORMANT_CONFIG = 12369;
    public const int TRANSPARENT_RGB = 12370;
    public const int RGB_BUFFER = 12430;
    public const int LUMINANCE_BUFFER = 12431;
    public const int NO_TEXTURE = 12380;
    public const int TEXTURE_RGB = 12381;
    public const int TEXTURE_RGBA = 12382;
    public const int TEXTURE_2D = 12383;
    public const int PBUFFER_BIT = 1;
    public const int PIXMAP_BIT = 2;
    public const int WINDOW_BIT = 4;
    public const int VG_COLORSPACE_LINEAR_BIT = 32;
    public const int VG_ALPHA_FORMAT_PRE_BIT = 64;
    public const int MULTISAMPLE_RESOLVE_BOX_BIT = 512;
    public const int SWAP_BEHAVIOR_PRESERVED_BIT = 1024;
    public const int OPENGL_ES_BIT = 1;
    public const int OPENVG_BIT = 2;
    public const int OPENGL_ES2_BIT = 4;
    public const int OPENGL_BIT = 8;
    public const int OPENGL_ES3_BIT = 64;
    public const int VENDOR = 12371;
    public const int VERSION = 12372;
    public const int EXTENSIONS = 12373;
    public const int CLIENT_APIS = 12429;
    public const int HEIGHT = 12374;
    public const int WIDTH = 12375;
    public const int LARGEST_PBUFFER = 12376;
    public const int TEXTURE_FORMAT = 12416;
    public const int TEXTURE_TARGET = 12417;
    public const int MIPMAP_TEXTURE = 12418;
    public const int MIPMAP_LEVEL = 12419;
    public const int RENDER_BUFFER = 12422;
    public const int VG_COLORSPACE = 12423;
    public const int VG_ALPHA_FORMAT = 12424;
    public const int HORIZONTAL_RESOLUTION = 12432;
    public const int VERTICAL_RESOLUTION = 12433;
    public const int PIXEL_ASPECT_RATIO = 12434;
    public const int SWAP_BEHAVIOR = 12435;
    public const int MULTISAMPLE_RESOLVE = 12441;
    public const int BACK_BUFFER = 12420;
    public const int SINGLE_BUFFER = 12421;
    public const int VG_COLORSPACE_sRGB = 12425;
    public const int VG_COLORSPACE_LINEAR = 12426;
    public const int VG_ALPHA_FORMAT_NONPRE = 12427;
    public const int VG_ALPHA_FORMAT_PRE = 12428;
    public const int DISPLAY_SCALING = 10000;
    public const int UNKNOWN = -1;
    public const int BUFFER_PRESERVED = 12436;
    public const int BUFFER_DESTROYED = 12437;
    public const int OPENVG_IMAGE = 12438;
    public const int CONTEXT_CLIENT_TYPE = 12439;
    public const int CONTEXT_CLIENT_VERSION = 12440;
    public const int MULTISAMPLE_RESOLVE_DEFAULT = 12442;
    public const int MULTISAMPLE_RESOLVE_BOX = 12443;
    public const int OPENGL_ES_API = 12448;
    public const int OPENVG_API = 12449;
    public const int OPENGL_API = 12450;
    public const int DRAW = 12377;
    public const int READ = 12378;
    public const int CORE_NATIVE_ENGINE = 12379;
    public const int COLORSPACE = VG_COLORSPACE;
    public const int ALPHA_FORMAT = VG_ALPHA_FORMAT;
    public const int COLORSPACE_sRGB = VG_COLORSPACE_sRGB;
    public const int COLORSPACE_LINEAR = VG_COLORSPACE_LINEAR;
    public const int ALPHA_FORMAT_NONPRE = VG_ALPHA_FORMAT_NONPRE;
    public const int ALPHA_FORMAT_PRE = VG_ALPHA_FORMAT_PRE;
    // EGL_KHR_create_context extension
    public const int CONTEXT_MAJOR_VERSION_KHR = 0x3098;
    public const int CONTEXT_MINOR_VERSION_KHR = 0x30FB;
    public const int CONTEXT_OPENGL_PROFILE_MASK_KHR = 0x30FD;
    public const int CONTEXT_OPENGL_RESET_NOTIFICATION_STRATEGY_KHR = 0x31BD;
    public const int NO_RESET_NOTIFICATION_KHR = 0x31BE;
    public const int LOSE_CONTEXT_ON_RESET_KHR = 0x31BF;
    public const int CONTEXT_OPENGL_DEBUG_BIT_KHR = 0x00000001;
    public const int CONTEXT_OPENGL_FORWARD_COMPATIBLE_BIT_KHR = 0x00000002;
    public const int CONTEXT_OPENGL_ROBUST_ACCESS_BIT_KHR = 0x00000004;
    public const int CONTEXT_OPENGL_CORE_PROFILE_BIT_KHR = 0x00000001;
    public const int CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR = 0x00000002;
    public const int OPENGL_ES3_BIT_KHR = 0x00000040;
    
    // EGL_ANGLE_d3d_share_handle_client_buffer
    public const int D3D_TEXTURE_2D_SHARE_HANDLE_ANGLE = 0x3200;

    // EGL_ANGLE_window_fixed_size
    public const int FIXED_SIZE_ANGLE = 0x3201;

    // EGL_ANGLE_query_surface_pointer
    [DllImport(LibName, EntryPoint = "eglQuerySurfacePointerANGLE")]
    public static extern bool QuerySurfacePointerANGLE(EGLDisplay display, EGLSurface surface, int attribute,
        out IntPtr value);

    [DllImport(LibName, EntryPoint = "eglGetPlatformDisplayEXT")]
    public static extern EGLDisplay GetPlatformDisplay(int platform, EGLNativeDisplayType displayId, int[] attribList);

    // EGL_ANGLE_software_display
    public static readonly EGLNativeDisplayType SOFTWARE_DISPLAY_ANGLE = new EGLNativeDisplayType(-1);

    // EGL_ANGLE_direct3d_display
    public static readonly EGLNativeDisplayType D3D11_ELSE_D3D9_DISPLAY_ANGLE = new EGLNativeDisplayType(-2);

    public static readonly EGLNativeDisplayType D3D11_ONLY_DISPLAY_ANGLE = new EGLNativeDisplayType(-3);

    // EGL_ANGLE_device_d3d
    public const int D3D9_DEVICE_ANGLE = 0x33A0;

    public const int D3D11_DEVICE_ANGLE = 0x33A1;

    // EGL_ANGLE_platform_angle
    public const int PLATFORM_ANGLE_ANGLE = 0x3202;
    public const int PLATFORM_ANGLE_TYPE_ANGLE = 0x3203;
    public const int PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE = 0x3204;
    public const int PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE = 0x3205;

    public const int PLATFORM_ANGLE_TYPE_DEFAULT_ANGLE = 0x3206;

    // EGL_ANGLE_platform_angle_d3d
    public const int PLATFORM_ANGLE_TYPE_D3D9_ANGLE = 0x3207;
    public const int PLATFORM_ANGLE_TYPE_D3D11_ANGLE = 0x3208;
    public const int PLATFORM_ANGLE_DEVICE_TYPE_ANGLE = 0x3209;
    public const int PLATFORM_ANGLE_DEVICE_TYPE_HARDWARE_ANGLE = 0x320A;
    public const int PLATFORM_ANGLE_DEVICE_TYPE_WARP_ANGLE = 0x320B;
    public const int PLATFORM_ANGLE_DEVICE_TYPE_REFERENCE_ANGLE = 0x320C;

    public const int PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE = 0x320F;

    // EGL_ANGLE_platform_angle_opengl
    public const int PLATFORM_ANGLE_TYPE_OPENGL_ANGLE = 0x320D;

    public const int PLATFORM_ANGLE_TYPE_OPENGLES_ANGLE = 0x320E;

    // See EGL_ANGLE_surface_d3d_texture_2d_share_handle
    public const int EGL_D3D_TEXTURE_2D_SHARE_HANDLE_ANGLE = 0x3200;

    [DllImport(LibName, EntryPoint = "eglGetError")]
    public static extern ErrorCode GetError();

    [DllImport(LibName, EntryPoint = "eglGetDisplay")]
    public static extern EGLDisplay GetDisplay(EGLNativeDisplayType display_id);

    [DllImport(LibName, EntryPoint = "eglInitialize")]
    //[return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool Initialize(EGLDisplay dpy, out int major, out int minor);

    [DllImport(LibName, EntryPoint = "eglTerminate")]
    //[return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool Terminate(EGLDisplay dpy);

    [DllImport(LibName, EntryPoint = "eglQueryString")]
    public static extern IntPtr QueryString(EGLDisplay dpy, int name);

    [DllImport(LibName, EntryPoint = "eglGetConfigs")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool GetConfigs(EGLDisplay dpy, EGLConfig[]? configs, int config_size, out int num_config);

    [DllImport(LibName, EntryPoint = "eglChooseConfig")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ChooseConfig(EGLDisplay dpy, int[] attrib_list, [In, Out] EGLConfig[] configs,
        int config_size, out int num_config);

    [DllImport(LibName, EntryPoint = "eglGetConfigAttrib")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool GetConfigAttrib(EGLDisplay dpy, EGLConfig config, int attribute, out int value);

    [DllImport(LibName, EntryPoint = "eglCreateWindowSurface")]
    public static extern EGLSurface CreateWindowSurface(EGLDisplay dpy, EGLConfig config, IntPtr win,
        int[] attrib_list);

    [DllImport(LibName, EntryPoint = "eglCreatePbufferSurface")]
    public static extern EGLSurface CreatePbufferSurface(EGLDisplay dpy, EGLConfig config, int[] attrib_list);

    [DllImport(LibName, EntryPoint = "eglCreatePixmapSurface")]
    public static extern EGLSurface CreatePixmapSurface(EGLDisplay dpy, EGLConfig config, EGLNativePixmapType pixmap,
        int[] attrib_list);

    [DllImport(LibName, EntryPoint = "eglDestroySurface")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool DestroySurface(EGLDisplay dpy, EGLSurface surface);

    [DllImport(LibName, EntryPoint = "eglQuerySurface")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool QuerySurface(EGLDisplay dpy, EGLSurface surface, int attribute, out int value);

    [DllImport(LibName, EntryPoint = "eglBindAPI")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool BindAPI(RenderApi api);

    [DllImport(LibName, EntryPoint = "eglQueryAPI")]
    public static extern int QueryAPI();

    [DllImport(LibName, EntryPoint = "eglWaitClient")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool WaitClient();

    [DllImport(LibName, EntryPoint = "eglReleaseThread")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ReleaseThread();

    [DllImport(LibName, EntryPoint = "eglCreatePbufferFromClientBuffer")]
    public static extern EGLSurface CreatePbufferFromClientBuffer(EGLDisplay dpy, int buftype, EGLClientBuffer buffer,
        EGLConfig config, int[] attrib_list);

    [DllImport(LibName, EntryPoint = "eglSurfaceAttrib")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SurfaceAttrib(EGLDisplay dpy, EGLSurface surface, int attribute, int value);

    [DllImport(LibName, EntryPoint = "eglBindTexImage")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool BindTexImage(EGLDisplay dpy, EGLSurface surface, int buffer);

    [DllImport(LibName, EntryPoint = "eglReleaseTexImage")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ReleaseTexImage(EGLDisplay dpy, EGLSurface surface, int buffer);

    [DllImport(LibName, EntryPoint = "eglSwapInterval")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SwapInterval(EGLDisplay dpy, int interval);

    [DllImport(LibName, EntryPoint = "eglCreateContext")]
    private static extern IntPtr eglCreateContext(EGLDisplay dpy, EGLConfig config, EGLContext share_context,
        int[] attrib_list);

    public static EGLContext CreateContext(EGLDisplay dpy, EGLConfig config, EGLContext share_context,
        int[] attrib_list)
    {
        IntPtr ptr = eglCreateContext(dpy, config, share_context, attrib_list);
        if (ptr == IntPtr.Zero)
        {
            throw new ApplicationException("EGL context creation failed");
        }

        return ptr;
    }

    [DllImport(LibName, EntryPoint = "eglDestroyContext")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool DestroyContext(EGLDisplay dpy, EGLContext ctx);

    [DllImport(LibName, EntryPoint = "eglMakeCurrent")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool MakeCurrent(EGLDisplay dpy, EGLSurface draw, EGLSurface read, EGLContext ctx);

    [DllImport(LibName, EntryPoint = "eglGetCurrentContext")]
    public static extern EGLContext GetCurrentContext();

    [DllImport(LibName, EntryPoint = "eglGetCurrentSurface")]
    public static extern EGLSurface GetCurrentSurface(int readdraw);

    [DllImport(LibName, EntryPoint = "eglGetCurrentDisplay")]
    public static extern EGLDisplay GetCurrentDisplay();

    [DllImport(LibName, EntryPoint = "eglQueryContext")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool QueryContext(EGLDisplay dpy, EGLContext ctx, int attribute, out int value);

    [DllImport(LibName, EntryPoint = "eglWaitGL")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool WaitGL();

    [DllImport(LibName, EntryPoint = "eglWaitNative")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool WaitNative(int engine);

    [DllImport(LibName, EntryPoint = "eglSwapBuffers")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SwapBuffers(EGLDisplay dpy, EGLSurface surface);

    [DllImport(LibName, EntryPoint = "eglCopyBuffers")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool CopyBuffers(EGLDisplay dpy, EGLSurface surface, EGLNativePixmapType target);

    [DllImport(LibName, EntryPoint = "eglGetProcAddress")]
    public static extern IntPtr GetProcAddress(string funcname);

    [DllImport(LibName, EntryPoint = "eglGetProcAddress")]
    public static extern IntPtr GetProcAddress(IntPtr funcname);

    // EGL_EXT_platform_base
    [DllImport(LibName, EntryPoint = "eglGetPlatformDisplayEXT")]
    public static extern EGLDisplay GetPlatformDisplayEXT(int platform, EGLNativeDisplayType native_display,
        int[] attrib_list);

    [DllImport(LibName, EntryPoint = "eglCreatePlatformWindowSurfaceEXT")]
    public static extern EGLSurface CreatePlatformWindowSurfaceEXT(EGLDisplay dpy, EGLConfig config,
        EGLNativeWindowType native_window, int[] attrib_list);

    [DllImport(LibName, EntryPoint = "eglCreatePlatformPixmapSurfaceEXT")]
    public static extern EGLSurface CreatePlatformPixmapSurfaceEXT(EGLDisplay dpy, EGLConfig config,
        EGLNativePixmapType native_pixmap, int[] attrib_list);

    // Returns true if Egl drivers exist on the system.
    public static bool IsSupported
    {
        get
        {
            try
            {
                GetCurrentContext();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}