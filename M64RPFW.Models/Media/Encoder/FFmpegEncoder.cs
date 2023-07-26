using FFmpeg.AutoGen;
using M64RPFW.Models.Media.Helpers;
using static FFmpeg.AutoGen.AVPixelFormat;
using static FFmpeg.AutoGen.ffmpeg;

namespace M64RPFW.Models.Media.Encoder;

/// <summary>
/// Large complicated class handling all encoder logic.
/// </summary>
public unsafe partial class FFmpegEncoder
{
    private AVFormatContext* _fmtCtx;

    // Video stuff
    private AVStream* _vStream;
    private long _vPts;
    private AVCodecContext* _vCodecCtx;
    private AVCodec* _vCodec;
    private AVPacket* _vPacket;
    private AVFrame* _vFrame1;
    private AVFrame* _vFrame2;
    private SwsContext* _sws;
    private SemaphoreSlim _vSem;

    // Audio stuff
    private AVStream* _aStream;
    private long _aPts;
    private AVCodecContext* _aCodecCtx;
    private AVCodec* _aCodec;
    private AVPacket* _aPacket;
    private AVFrame* _aFrame1;
    private AVFrame* _aFrame2;
    private SwrContext* _swr;
    private int _aFrameSize;
    private SemaphoreSlim _aSem;

    public FFmpegEncoder(string path, string? mimeType, FFmpegConfig? config = null)
    {
        int err;
        AVOutputFormat* ofmt = SelectOutputFormat(path, mimeType);
        
        fixed (AVFormatContext** pFmtCtx = &_fmtCtx)
        {
            if ((err = avformat_alloc_output_context2(pFmtCtx, ofmt, null, path)) < 0)
                throw new AVException(err);
        }
    }

    private AVOutputFormat* SelectOutputFormat(string path, string? fmt)
    {
        int err;
        AVOutputFormat* ofmt = fmt == null ? 
            av_guess_format(null, path, null) : 
            av_guess_format(null, null, fmt);

        if (ofmt == null)
            throw new ArgumentException("No output format available");

        return ofmt;
    }

    public void Dispose()
    {
        if (_vCodec != null)
        {
            AVHelpers.Dispose(ref _vCodecCtx);
            AVHelpers.Dispose(ref _vPacket);
            AVHelpers.Dispose(ref _vFrame1);
            AVHelpers.Dispose(ref _vFrame2);
            AVHelpers.Dispose(ref _sws);
        }
        if (_aCodec != null)
        {
            AVHelpers.Dispose(ref _aCodecCtx);
            AVHelpers.Dispose(ref _aPacket);
            AVHelpers.Dispose(ref _aFrame1);
            AVHelpers.Dispose(ref _aFrame2);
            AVHelpers.Dispose(ref _swr);
        }
        
        AVHelpers.Dispose(ref _fmtCtx);
    }
}