using System.Text;

namespace M64RPFW.Models.Helpers;

/// <summary>
/// Helpers that deal with C types and objects.
/// </summary>
public static unsafe class CHelpers
{
    public static string DecodeString(byte* b, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetString(b, (int) strlen(b));
    }
    
    
    public static long strlen(byte* b)
    {
        byte* p = b;
        while (*p != 0) p++;
        return p - b;
    }

    public static void memcpy(void* dst, void* src, ulong n)
    {
        Buffer.MemoryCopy(src, dst, n, n);
    }
}

/// <summary>
/// Class holding every possible errno value.
/// </summary>
public static class Errno
{
    /// <summary>
    /// Operation not permitted
    /// </summary>
    public const int EPERM = 1;

    /// <summary>
    /// No such file or directory
    /// </summary>
    public const int ENOENT = 2;

    /// <summary>
    /// No such process
    /// </summary>
    public const int ESRCH = 3;

    /// <summary>
    /// Interrupted system call
    /// </summary>
    public const int EINTR = 4;

    /// <summary>
    /// Input/output error
    /// </summary>
    public const int EIO = 5;

    /// <summary>
    /// No such device or address
    /// </summary>
    public const int ENXIO = 6;

    /// <summary>
    /// Argument list too long
    /// </summary>
    public const int E2BIG = 7;

    /// <summary>
    /// Exec format error
    /// </summary>
    public const int ENOEXEC = 8;

    /// <summary>
    /// Bad file descriptor
    /// </summary>
    public const int EBADF = 9;

    /// <summary>
    /// No child processes
    /// </summary>
    public const int ECHILD = 10;

    /// <summary>
    /// Resource temporarily unavailable
    /// </summary>
    public const int EAGAIN = 11;

    /// <summary>
    /// Resource temporarily unavailable
    /// </summary>
    public const int EWOULDBLOCK = 11;

    /// <summary>
    /// Cannot allocate memory
    /// </summary>
    public const int ENOMEM = 12;

    /// <summary>
    /// Permission denied
    /// </summary>
    public const int EACCES = 13;

    /// <summary>
    /// Bad address
    /// </summary>
    public const int EFAULT = 14;

    /// <summary>
    /// Block device required
    /// </summary>
    public const int ENOTBLK = 15;

    /// <summary>
    /// Device or resource busy
    /// </summary>
    public const int EBUSY = 16;

    /// <summary>
    /// File exists
    /// </summary>
    public const int EEXIST = 17;

    /// <summary>
    /// Invalid cross-device link
    /// </summary>
    public const int EXDEV = 18;

    /// <summary>
    /// No such device
    /// </summary>
    public const int ENODEV = 19;

    /// <summary>
    /// Not a directory
    /// </summary>
    public const int ENOTDIR = 20;

    /// <summary>
    /// Is a directory
    /// </summary>
    public const int EISDIR = 21;

    /// <summary>
    /// Invalid argument
    /// </summary>
    public const int EINVAL = 22;

    /// <summary>
    /// Too many open files in system
    /// </summary>
    public const int ENFILE = 23;

    /// <summary>
    /// Too many open files
    /// </summary>
    public const int EMFILE = 24;

    /// <summary>
    /// Inappropriate ioctl for device
    /// </summary>
    public const int ENOTTY = 25;

    /// <summary>
    /// Text file busy
    /// </summary>
    public const int ETXTBSY = 26;

    /// <summary>
    /// File too large
    /// </summary>
    public const int EFBIG = 27;

    /// <summary>
    /// No space left on device
    /// </summary>
    public const int ENOSPC = 28;

    /// <summary>
    /// Illegal seek
    /// </summary>
    public const int ESPIPE = 29;

    /// <summary>
    /// Read-only file system
    /// </summary>
    public const int EROFS = 30;

    /// <summary>
    /// Too many links
    /// </summary>
    public const int EMLINK = 31;

    /// <summary>
    /// Broken pipe
    /// </summary>
    public const int EPIPE = 32;

    /// <summary>
    /// Numerical argument out of domain
    /// </summary>
    public const int EDOM = 33;

    /// <summary>
    /// Numerical result out of range
    /// </summary>
    public const int ERANGE = 34;

    /// <summary>
    /// Resource deadlock avoided
    /// </summary>
    public const int EDEADLK = 35;

    /// <summary>
    /// Resource deadlock avoided
    /// </summary>
    public const int EDEADLOCK = 35;

    /// <summary>
    /// File name too long
    /// </summary>
    public const int ENAMETOOLONG = 36;

    /// <summary>
    /// No locks available
    /// </summary>
    public const int ENOLCK = 37;

    /// <summary>
    /// Function not implemented
    /// </summary>
    public const int ENOSYS = 38;

    /// <summary>
    /// Directory not empty
    /// </summary>
    public const int ENOTEMPTY = 39;

    /// <summary>
    /// Too many levels of symbolic links
    /// </summary>
    public const int ELOOP = 40;

    /// <summary>
    /// No message of desired type
    /// </summary>
    public const int ENOMSG = 42;

    /// <summary>
    /// Identifier removed
    /// </summary>
    public const int EIDRM = 43;

    /// <summary>
    /// Channel number out of range
    /// </summary>
    public const int ECHRNG = 44;

    /// <summary>
    /// Level 2 not synchronized
    /// </summary>
    public const int EL2NSYNC = 45;

    /// <summary>
    /// Level 3 halted
    /// </summary>
    public const int EL3HLT = 46;

    /// <summary>
    /// Level 3 reset
    /// </summary>
    public const int EL3RST = 47;

    /// <summary>
    /// Link number out of range
    /// </summary>
    public const int ELNRNG = 48;

    /// <summary>
    /// Protocol driver not attached
    /// </summary>
    public const int EUNATCH = 49;

    /// <summary>
    /// No CSI structure available
    /// </summary>
    public const int ENOCSI = 50;

    /// <summary>
    /// Level 2 halted
    /// </summary>
    public const int EL2HLT = 51;

    /// <summary>
    /// Invalid exchange
    /// </summary>
    public const int EBADE = 52;

    /// <summary>
    /// Invalid request descriptor
    /// </summary>
    public const int EBADR = 53;

    /// <summary>
    /// Exchange full
    /// </summary>
    public const int EXFULL = 54;

    /// <summary>
    /// No anode
    /// </summary>
    public const int ENOANO = 55;

    /// <summary>
    /// Invalid request code
    /// </summary>
    public const int EBADRQC = 56;

    /// <summary>
    /// Invalid slot
    /// </summary>
    public const int EBADSLT = 57;

    /// <summary>
    /// Bad font file format
    /// </summary>
    public const int EBFONT = 59;

    /// <summary>
    /// Device not a stream
    /// </summary>
    public const int ENOSTR = 60;

    /// <summary>
    /// No data available
    /// </summary>
    public const int ENODATA = 61;

    /// <summary>
    /// Timer expired
    /// </summary>
    public const int ETIME = 62;

    /// <summary>
    /// Out of streams resources
    /// </summary>
    public const int ENOSR = 63;

    /// <summary>
    /// Machine is not on the network
    /// </summary>
    public const int ENONET = 64;

    /// <summary>
    /// Package not installed
    /// </summary>
    public const int ENOPKG = 65;

    /// <summary>
    /// Object is remote
    /// </summary>
    public const int EREMOTE = 66;

    /// <summary>
    /// Link has been severed
    /// </summary>
    public const int ENOLINK = 67;

    /// <summary>
    /// Advertise error
    /// </summary>
    public const int EADV = 68;

    /// <summary>
    /// Srmount error
    /// </summary>
    public const int ESRMNT = 69;

    /// <summary>
    /// Communication error on send
    /// </summary>
    public const int ECOMM = 70;

    /// <summary>
    /// Protocol error
    /// </summary>
    public const int EPROTO = 71;

    /// <summary>
    /// Multihop attempted
    /// </summary>
    public const int EMULTIHOP = 72;

    /// <summary>
    /// RFS specific error
    /// </summary>
    public const int EDOTDOT = 73;

    /// <summary>
    /// Bad message
    /// </summary>
    public const int EBADMSG = 74;

    /// <summary>
    /// Value too large for defined data type
    /// </summary>
    public const int EOVERFLOW = 75;

    /// <summary>
    /// Name not unique on network
    /// </summary>
    public const int ENOTUNIQ = 76;

    /// <summary>
    /// File descriptor in bad state
    /// </summary>
    public const int EBADFD = 77;

    /// <summary>
    /// Remote address changed
    /// </summary>
    public const int EREMCHG = 78;

    /// <summary>
    /// Can not access a needed shared library
    /// </summary>
    public const int ELIBACC = 79;

    /// <summary>
    /// Accessing a corrupted shared library
    /// </summary>
    public const int ELIBBAD = 80;

    /// <summary>
    /// .lib section in a.out corrupted
    /// </summary>
    public const int ELIBSCN = 81;

    /// <summary>
    /// Attempting to link in too many shared libraries
    /// </summary>
    public const int ELIBMAX = 82;

    /// <summary>
    /// Cannot exec a shared library directly
    /// </summary>
    public const int ELIBEXEC = 83;

    /// <summary>
    /// Invalid or incomplete multibyte or wide character
    /// </summary>
    public const int EILSEQ = 84;

    /// <summary>
    /// Interrupted system call should be restarted
    /// </summary>
    public const int ERESTART = 85;

    /// <summary>
    /// Streams pipe error
    /// </summary>
    public const int ESTRPIPE = 86;

    /// <summary>
    /// Too many users
    /// </summary>
    public const int EUSERS = 87;

    /// <summary>
    /// Socket operation on non-socket
    /// </summary>
    public const int ENOTSOCK = 88;

    /// <summary>
    /// Destination address required
    /// </summary>
    public const int EDESTADDRREQ = 89;

    /// <summary>
    /// Message too long
    /// </summary>
    public const int EMSGSIZE = 90;

    /// <summary>
    /// Protocol wrong type for socket
    /// </summary>
    public const int EPROTOTYPE = 91;

    /// <summary>
    /// Protocol not available
    /// </summary>
    public const int ENOPROTOOPT = 92;

    /// <summary>
    /// Protocol not supported
    /// </summary>
    public const int EPROTONOSUPPORT = 93;

    /// <summary>
    /// Socket type not supported
    /// </summary>
    public const int ESOCKTNOSUPPORT = 94;

    /// <summary>
    /// Operation not supported
    /// </summary>
    public const int ENOTSUP = 95;

    /// <summary>
    /// Operation not supported
    /// </summary>
    public const int EOPNOTSUPP = 95;

    /// <summary>
    /// Protocol family not supported
    /// </summary>
    public const int EPFNOSUPPORT = 96;

    /// <summary>
    /// Address family not supported by protocol
    /// </summary>
    public const int EAFNOSUPPORT = 97;

    /// <summary>
    /// Address already in use
    /// </summary>
    public const int EADDRINUSE = 98;

    /// <summary>
    /// Cannot assign requested address
    /// </summary>
    public const int EADDRNOTAVAIL = 99;

    /// <summary>
    /// Network is down
    /// </summary>
    public const int ENETDOWN = 100;

    /// <summary>
    /// Network is unreachable
    /// </summary>
    public const int ENETUNREACH = 101;

    /// <summary>
    /// Network dropped connection on reset
    /// </summary>
    public const int ENETRESET = 102;

    /// <summary>
    /// Software caused connection abort
    /// </summary>
    public const int ECONNABORTED = 103;

    /// <summary>
    /// Connection reset by peer
    /// </summary>
    public const int ECONNRESET = 104;

    /// <summary>
    /// No buffer space available
    /// </summary>
    public const int ENOBUFS = 105;

    /// <summary>
    /// Transport endpoint is already connected
    /// </summary>
    public const int EISCONN = 106;

    /// <summary>
    /// Transport endpoint is not connected
    /// </summary>
    public const int ENOTCONN = 107;

    /// <summary>
    /// Cannot send after transport endpoint shutdown
    /// </summary>
    public const int ESHUTDOWN = 108;

    /// <summary>
    /// Too many references: cannot splice
    /// </summary>
    public const int ETOOMANYREFS = 109;

    /// <summary>
    /// Connection timed out
    /// </summary>
    public const int ETIMEDOUT = 110;

    /// <summary>
    /// Connection refused
    /// </summary>
    public const int ECONNREFUSED = 111;

    /// <summary>
    /// Host is down
    /// </summary>
    public const int EHOSTDOWN = 112;

    /// <summary>
    /// No route to host
    /// </summary>
    public const int EHOSTUNREACH = 113;

    /// <summary>
    /// Operation already in progress
    /// </summary>
    public const int EALREADY = 114;

    /// <summary>
    /// Operation now in progress
    /// </summary>
    public const int EINPROGRESS = 115;

    /// <summary>
    /// Stale file handle
    /// </summary>
    public const int ESTALE = 116;

    /// <summary>
    /// Structure needs cleaning
    /// </summary>
    public const int EUCLEAN = 117;

    /// <summary>
    /// Not a XENIX named type file
    /// </summary>
    public const int ENOTNAM = 118;

    /// <summary>
    /// No XENIX semaphores available
    /// </summary>
    public const int ENAVAIL = 119;

    /// <summary>
    /// Is a named type file
    /// </summary>
    public const int EISNAM = 120;

    /// <summary>
    /// Remote I/O error
    /// </summary>
    public const int EREMOTEIO = 121;

    /// <summary>
    /// Disk quota exceeded
    /// </summary>
    public const int EDQUOT = 122;

    /// <summary>
    /// No medium found
    /// </summary>
    public const int ENOMEDIUM = 123;

    /// <summary>
    /// Wrong medium type
    /// </summary>
    public const int EMEDIUMTYPE = 124;

    /// <summary>
    /// Operation canceled
    /// </summary>
    public const int ECANCELED = 125;

    /// <summary>
    /// Required key not available
    /// </summary>
    public const int ENOKEY = 126;

    /// <summary>
    /// Key has expired
    /// </summary>
    public const int EKEYEXPIRED = 127;

    /// <summary>
    /// Key has been revoked
    /// </summary>
    public const int EKEYREVOKED = 128;

    /// <summary>
    /// Key was rejected by service
    /// </summary>
    public const int EKEYREJECTED = 129;

    /// <summary>
    /// Owner died
    /// </summary>
    public const int EOWNERDEAD = 130;

    /// <summary>
    /// State not recoverable
    /// </summary>
    public const int ENOTRECOVERABLE = 131;

    /// <summary>
    /// Operation not possible due to RF-kill
    /// </summary>
    public const int ERFKILL = 132;

    /// <summary>
    /// Memory page has hardware error
    /// </summary>
    public const int EHWPOISON = 133;
}