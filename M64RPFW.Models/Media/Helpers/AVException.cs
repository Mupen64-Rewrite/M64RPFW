using System.Text;
using M64RPFW.Models.Helpers;
using static FFmpeg.AutoGen.ffmpeg;

namespace M64RPFW.Models.Media.Helpers;

/// <summary>
/// Error returned by an FFmpeg function.
/// </summary>
public class AVException : SystemException
{

    private static unsafe string AVStrErrorString(int errorCode)
    {
        byte[] buffer = new byte[AV_ERROR_MAX_STRING_SIZE];
        fixed (byte* pBuffer = buffer)
        {
            
            av_strerror(errorCode, pBuffer, (ulong) buffer.LongLength);
            return Encoding.UTF8.GetString(pBuffer, (int) CHelpers.strlen(pBuffer));
        }
    }

    /// <summary>
    /// Constructs an AVException from an error code.
    /// </summary>
    /// <param name="errorCode">The error code returned by FFmpeg</param>
    public AVException(int errorCode) : base(AVStrErrorString(errorCode))
    {
        ErrorCode = errorCode;
    }
    
    /// <summary>
    /// Constructs an AVException from an error code, with an optional cause.
    /// </summary>
    /// <param name="errorCode">The error code returned by FFmpeg</param>
    /// <param name="inner">The exception that caused this exception</param>
    public AVException(int errorCode, Exception inner) : base(AVStrErrorString(errorCode), inner)
    {
        ErrorCode = errorCode;
    }

    public int ErrorCode { get; }
}