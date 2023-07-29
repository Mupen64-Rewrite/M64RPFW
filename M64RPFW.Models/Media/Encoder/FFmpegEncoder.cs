using FFmpeg.AutoGen;
using M64RPFW.Models.Media.Helpers;
using M64RPFW.Services;
using static FFmpeg.AutoGen.ffmpeg;

namespace M64RPFW.Models.Media.Encoder;

/// <summary>
/// Large complicated class handling all encoder logic.
/// </summary>
public unsafe partial class FFmpegEncoder : IVideoEncoder
{
    private AVFormatContext* _fmtCtx;

    private VideoStream? _videoStream;
    private AudioStream? _audioStream;

    public FFmpegEncoder(string path, string? mimeType, FFmpegConfig? config = null)
    {
        int err;
        AVOutputFormat* ofmt = SelectOutputFormat(path, mimeType);
        
        fixed (AVFormatContext** pFmtCtx = &_fmtCtx)
        {
            if ((err = avformat_alloc_output_context2(pFmtCtx, ofmt, null, path)) < 0)
                throw new AVException(err);
        }
        
        InitCodecs(ofmt, config);

        if ((ofmt->flags & AVFMT_GLOBALHEADER) != 0)
            _fmtCtx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;

        if ((ofmt->flags & AVFMT_NOFILE) == 0)
        {
            if ((err = avio_open(&_fmtCtx->pb, path, AVIO_FLAG_WRITE)) < 0)
                throw new AVException(err);
        }

        AVDictionary* dict = AVHelpers.ToAVDictionary(config?.FormatOptions);
        try
        {
            if ((err = avformat_write_header(_fmtCtx, &dict)) < 0)
                throw new AVException(err);
        }
        catch
        {
            _videoStream?.Dispose();
            _audioStream?.Dispose();
            throw;
        }
        finally
        {
            av_dict_free(&dict);
        }
    }

    private static AVOutputFormat* SelectOutputFormat(string path, string? fmt)
    {
        int err;
        AVOutputFormat* ofmt = fmt == null ? 
            av_guess_format(null, path, null) : 
            av_guess_format(null, null, fmt);

        if (ofmt == null)
            throw new ArgumentException("No output format available");

        return ofmt;
    }

    private void InitCodecs(AVOutputFormat* ofmt, FFmpegConfig? config = null)
    {
        string? vCodecName = null, aCodecName = null;
        if (config != null)
        {
            vCodecName = config.FormatOptions.GetValueOrDefault("video_codec");
            aCodecName = config.FormatOptions.GetValueOrDefault("audio_codec");
        }

        AVCodec* vCodec, aCodec;
        
        vCodec = AVHelpers.CheckCodec(ofmt, AVMediaType.AVMEDIA_TYPE_VIDEO, vCodecName);
        if (vCodec == null)
            vCodec = AVHelpers.DefaultCodec(ofmt, AVMediaType.AVMEDIA_TYPE_VIDEO);

        aCodec = AVHelpers.CheckCodec(ofmt, AVMediaType.AVMEDIA_TYPE_AUDIO, aCodecName);
        if (aCodec == null)
            aCodec = AVHelpers.DefaultCodec(ofmt, AVMediaType.AVMEDIA_TYPE_AUDIO);

        _audioStream = aCodec != null ? new AudioStream(_fmtCtx, aCodec, config?.AudioOptions) : null;
        _videoStream = vCodec != null ? new VideoStream(_fmtCtx, vCodec, _audioStream, config?.VideoOptions) : null;
    }
    
    public void ConsumeVideo(int width, int height, ICaptureService readScreen)
    {
        _videoStream?.ConsumeFrame(width, height, readScreen);
    }

    public void SetAudioSampleRate(int sampleRate)
    {
        _audioStream?.SetSampleRate(sampleRate);
    }

    public void ConsumeAudio(void* data, ulong len)
    {
        _audioStream?.ConsumeSamples(data, (int) len);
    }

    public void Finish()
    {
        int err;
        _videoStream?.Flush();
        _audioStream?.Flush();

        if ((err = av_write_trailer(_fmtCtx)) < 0)
            throw new AVException(err);
    }

    private void ReleaseUnmanagedResources()
    {
        AVHelpers.Dispose(ref _fmtCtx);
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _videoStream?.Dispose();
            _audioStream?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~FFmpegEncoder()
    {
        Dispose(false);
    }
}