using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public class IndependentGlControlResources : IDisposable
{
    private IOpenGlTextureSharingRenderInterfaceContextFeature _glSharing;
    private IGlContext _context;

    private GlBufferGroup? _front, _middle, _back;
    private uint _fbo = 0;

    private int _hasSwapped = 0;

    private static InternalFormat CalculateDepthFormat(int depthSize, GlVersion version)
    {
        return depthSize switch
        {
            >= 16 and < 24 => InternalFormat.DepthComponent16,
            >= 24 and < 32 => InternalFormat.DepthComponent24,
            32 => InternalFormat.DepthComponent32,
            _ => version.Type switch
            {
                GlProfileType.OpenGL => InternalFormat.DepthComponent,
                GlProfileType.OpenGLES => InternalFormat.DepthComponent16,
                _ => InternalFormat.DepthComponent16
            }
        };
    }

    public IndependentGlControlResources(IOpenGlTextureSharingRenderInterfaceContextFeature glSharing, GlVersion? preferredVersion = null)
    {
        _glSharing = glSharing;
        if (preferredVersion != null)
        {
            var val = preferredVersion.Value;
            Console.WriteLine($"Preferred version: OpenGL {(val.Type == GlProfileType.OpenGLES ? "ES " : "")}{val.Major}.{val.Minor}");
        }
        _context = glSharing.CreateSharedContext(preferredVersion.HasValue
            ? new[]
            {
                preferredVersion.Value
            }
            : null)!;
        using (_context.MakeCurrent())
        {
            Console.WriteLine($"Context version: {_context.GlInterface.Version}");
        }
    }

    public IDisposable MakeCurrent()
    {
        return _context.MakeCurrent();
    }

    public unsafe IntPtr GetProcAddress(string sym)
    {
        return _context.GlInterface.GetProcAddress(sym);
    }

    public void SwapBuffers(PixelSize size, int depth)
    {
        var gl = GL.GetApi(_context.GlInterface.GetProcAddress);

        gl.Finish();

        _back = Interlocked.Exchange(ref _middle, _back);
        _hasSwapped = 1;

        InitBuffers(size, depth);
    }

    private DebugProc? _debugFn;

    public void InitBuffers(PixelSize size, int depth, bool setViewport = false)
    {
        var gl = GL.GetApi(_context.GlInterface.GetProcAddress);

        _back ??= new GlBufferGroup(_glSharing, _context, size, CalculateDepthFormat(depth, _context.Version));

        if (_fbo == 0)
        {
            // InstallDebugLogger(gl);
            _fbo = gl.GenFramebuffer();
        }
        GLEnum err;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _back.AttachToCurrentFBO(gl);
        // if (setViewport)
        //     gl.Viewport(0, 0, (uint) size.Width, (uint) size.Height);

        gl.Flush();
    }

    private unsafe void InstallDebugLogger(GL gl)
    {
        if (_debugFn != null)
            return;
        _debugFn = delegate(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr param)
        {
            string sSource = (DebugSource) source switch
            {
                DebugSource.DebugSourceApi => "API   ",
                DebugSource.DebugSourceWindowSystem => "WINSYS",
                DebugSource.DebugSourceShaderCompiler => "SHADER",
                DebugSource.DebugSourceThirdParty => "3PARTY",
                DebugSource.DebugSourceApplication => "APP   ",
                DebugSource.DebugSourceOther => "OTHER "
            };
            string sType = (DebugType) type switch
            {
                DebugType.DebugTypeError => "ERROR",
                DebugType.DebugTypeDeprecatedBehavior => "DEPRC",
                DebugType.DebugTypeUndefinedBehavior => "UNDEF",
                DebugType.DebugTypePortability => "PORT ",
                DebugType.DebugTypePerformance => "PERF ",
                DebugType.DebugTypeMarker => "MARK ",
                DebugType.DebugTypePushGroup => "PUSH ",
                DebugType.DebugTypePopGroup => "POP  ",
                DebugType.DebugTypeOther => "OTHER",
            };
            string sSeverity = (DebugSeverity) severity switch
            {
                DebugSeverity.DebugSeverityHigh => "HIGH ",
                DebugSeverity.DebugSeverityMedium => "MED  ",
                DebugSeverity.DebugSeverityLow => "LOW  ",
                DebugSeverity.DebugSeverityNotification => "NOTE ",
            };
            string msg = Encoding.UTF8.GetString((byte*) message, length);

            Console.WriteLine($"GL[0x{(uint) id:X8} | {sSource} {sType} | {sSeverity}] {msg}");

        };
        gl.DebugMessageCallback(_debugFn, null);
        gl.Enable(EnableCap.DebugOutputSynchronous);
        gl.Enable(EnableCap.DebugOutput);
    }

    public async Task PresentNext(ICompositionGpuInterop interop, CompositionDrawingSurface surface)
    {
        if (Interlocked.Exchange(ref _hasSwapped, 0) != 0)
            _front = Interlocked.Exchange(ref _middle, _front);

        if (_front == null)
            return;

        var import = _front.Import(interop);
        await surface.UpdateAsync(import);
    }

    public uint? FBO => _fbo;

    public GlVersion ContextVersion => _context.Version;

    public void Dispose()
    {
        var gl = GL.GetApi(_context.GlInterface.GetProcAddress);

        _front?.Dispose();
        _middle?.Dispose();
        _back?.Dispose();

        gl.DeleteFramebuffer(_fbo);
        _context.Dispose();
    }
}