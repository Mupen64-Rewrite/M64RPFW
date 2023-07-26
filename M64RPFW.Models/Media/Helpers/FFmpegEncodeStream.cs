using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;
using static M64RPFW.Models.Media.Helpers.AVException;

namespace M64RPFW.Models.Media.Helpers;

/// <summary>
/// Common code for FFmpeg encoder streams.
/// </summary>
internal abstract unsafe class FFmpegEncodeStream : IDisposable
{
    protected delegate void StreamConfigCallback(AVCodecContext* codecCtx, AVStream* stream, AVCodec* codec);
    
    internal protected AVFormatContext* _fmtCtx;
    internal protected AVCodec* _codec;
    internal protected AVStream* _stream;

    internal protected long _pts;
    internal protected AVCodecContext* _codecCtx;
    internal protected AVPacket* _packet;
    
    /// <summary>
    /// Allocates and initializes a stream, codec context, and packet.
    /// </summary>
    /// <param name="fmtCtx">The <see cref="AVFormatContext"/> owning this stream.</param>
    /// <param name="codec">The <see cref="AVCodec"/> to use.</param>
    /// <param name="streamConfig">A function called before opening the context.</param>
    /// <param name="config">A dictionary of config options, passed to the codec context.</param>
    /// <exception cref="Exception">if a serious error occurs and there is no appropriate FFmpeg error code</exception>
    /// <exception cref="AVException">if an FFmpeg error occurs</exception>
    /// <remarks>
    /// This constructor should not be directly exposed to end users. Instead, subclasses should provide their own
    /// <see cref="StreamConfigCallback"/> to this constructor.
    /// </remarks>
    protected FFmpegEncodeStream(AVFormatContext* fmtCtx, AVCodec* codec, StreamConfigCallback streamConfig, IDictionary<string, string>? config)
    {
        int err;
        
        ArgumentNullException.ThrowIfNull(fmtCtx);
        ArgumentNullException.ThrowIfNull(codec);
        
        _fmtCtx = fmtCtx;
        _codec = codec;

        _stream = avformat_new_stream(_fmtCtx, _codec);
        _codecCtx = avcodec_alloc_context3(_codec);
        _packet = av_packet_alloc();
        _pts = 0;
        if (_stream == null || _codecCtx == null || _packet == null)
        {
            throw new Exception("FFmpeg allocations failed");
        }

        streamConfig(_codecCtx, _stream, _codec);

        var avDictionary = AVHelpers.ToAVDictionary(config);
        try
        {
            if ((err = avcodec_open2(_codecCtx, _codec, &avDictionary)) < 0)
                throw new AVException(err);
        }
        finally
        {
            av_dict_free(&avDictionary);
        }

        if ((err = avcodec_parameters_from_context(_stream->codecpar, _codecCtx)) < 0)
            throw new AVException(err);
    }

    /// <summary>
    /// Encodes a frame into this stream.
    /// </summary>
    /// <param name="frame">A frame, or null. If null, the stream is flushed.</param>
    protected void EncodeFrame(AVFrame* frame)
    {
        int err;
        // send in the frame
        if ((err = avcodec_send_frame(_codecCtx, frame)) < 0)
            throw new AVException(err);
        
        // extract as many packets as possible
        while ((err = avcodec_receive_packet(_codecCtx, _packet)) >= 0)
        {
            av_packet_rescale_ts(_packet, _codecCtx->time_base, _stream->time_base);
            _packet->stream_index = _stream->index;
            if ((err = av_interleaved_write_frame(_fmtCtx, _packet)) < 0)
                throw new AVException(err);
        }
        if (err != AVERROR(EAGAIN) && err != AVERROR_EOF)
            throw new AVException(err);
    }

    protected virtual void ReleaseUnmanagedResources()
    {
        AVHelpers.Dispose(ref _codecCtx);
        AVHelpers.Dispose(ref _packet);
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~FFmpegEncodeStream()
    {
        Dispose(false);
    }
}