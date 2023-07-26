using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using M64RPFW.Models.Helpers;
using static FFmpeg.AutoGen.ffmpeg;

namespace M64RPFW.Models.Media.Helpers;

/// <summary>
/// A bunch of utilities and extensions for FFmpeg objects.
/// </summary>
internal static unsafe class AVHelpers
{
    public static readonly AVChannelLayout AV_STEREO_CHANNEL_LAYOUT = new()
    {
        order = AVChannelOrder.AV_CHANNEL_ORDER_NATIVE,
        nb_channels = 2,
        u = new AVChannelLayout_u() { mask = AV_CH_LAYOUT_STEREO }
    };
    
    #region Easy dispose

    /// <summary>
    /// Disposes an FFmpeg object.
    /// </summary>
    /// <param name="fmtCtx">The AVFormatContext to dispose</param>
    public static void Dispose(ref AVFormatContext* fmtCtx)
    {
        avformat_free_context(fmtCtx);
        fmtCtx = null;
    }

    /// <summary>
    /// Disposes an FFmpeg object.
    /// </summary>
    /// <param name="codecCtx">The AVCodecContext to dispose</param>
    public static void Dispose(ref AVCodecContext* codecCtx)
    {
        fixed (AVCodecContext** pCodecCtx = &codecCtx)
        {
            avcodec_free_context(pCodecCtx);
        }
    }
    
    /// <summary>
    /// Disposes an FFmpeg object.
    /// </summary>
    /// <param name="frame">The AVFrame to dispose</param>
    public static void Dispose(ref AVFrame* frame)
    {
        fixed (AVFrame** pFrame = &frame)
        {
            av_frame_free(pFrame);
        }
    }
    
    /// <summary>
    /// Disposes an FFmpeg object.
    /// </summary>
    /// <param name="packet">The AVPacket to dispose</param>
    public static void Dispose(ref AVPacket* packet)
    {
        fixed (AVPacket** pPacket = &packet)
        {
            av_packet_free(pPacket);
        }
    }

    /// <summary>
    /// Disposes an FFmpeg object.
    /// </summary>
    /// <param name="sws">The SwsContext to dispose</param>
    public static void Dispose(ref SwsContext* sws)
    {
        sws_freeContext(sws);
        sws = null;
    }

    /// <summary>
    /// Disposes an FFmpeg object.
    /// </summary>
    /// <param name="swr">The SwrContext to dispose</param>
    public static void Dispose(ref SwrContext* swr)
    {
        fixed (SwrContext** pSwr = &swr)
        {
            swr_free(pSwr);
        }
    }

    #endregion

    /// <summary>
    /// Selects a codec based on the format and/or specified codec.
    /// </summary>
    /// <param name="ofmt">The output format</param>
    /// <param name="type">The stream type</param>
    /// <param name="reason">If returning null, holds the reason why a fallback is needed</param>
    /// <param name="name">A codec that </param>
    /// <returns>The desired codec, or null if a fallback is required.</returns>
    public static AVCodec* CheckCodec(AVOutputFormat* ofmt, AVMediaType type, out string? reason, string? name = null)
    {
        reason = null;
        if (name == null)
            return null;

        AVCodec* codec = avcodec_find_encoder_by_name(name);
        if (codec == null)
        {
            reason = $"No codec named \"{name}\" exists";
            return null;
        }
        if (codec->type != type)
        {
            var typeString = av_get_media_type_string(type);
            reason = $"Codec \"{name}\" is not {("aeiouy".Contains(typeString[0]) ? "an" : "a")} {typeString} codec";
            return null;
        }
        if (avformat_query_codec(ofmt, codec->id, FF_COMPLIANCE_EXPERIMENTAL) == 0)
        {
            reason = $"Format \"{CHelpers.DecodeString(ofmt->long_name)}\" does not support codec \"{name}\"";
            return null;
        }
        return codec;
    }

    /// <summary>
    /// Attempts to find a default codec for the given format and media type.
    /// </summary>
    /// <param name="ofmt">The output format</param>
    /// <param name="type">The stream type</param>
    /// <param name="reason">Returns a reason if the method fails, unless it's because there is no default codec.</param>
    /// <returns>The default codec, or null if no default codec exists or is supported.</returns>
    public static AVCodec* DefaultCodec(AVOutputFormat* ofmt, AVMediaType type, out string? reason)
    {
        reason = null;
        AVCodecID codecId;
        switch (type)
        {
            case AVMediaType.AVMEDIA_TYPE_AUDIO:
                codecId = ofmt->audio_codec;
                break;
            case AVMediaType.AVMEDIA_TYPE_VIDEO:
                codecId = ofmt->video_codec;
                break;
            case AVMediaType.AVMEDIA_TYPE_SUBTITLE:
                codecId = ofmt->subtitle_codec;
                break;
            default:
                reason = $"Formats have no default {av_get_media_type_string(type)} codec";
                return null;
        }
        if (codecId == AVCodecID.AV_CODEC_ID_NONE)
            return null;
        
        AVCodec* codec = avcodec_find_encoder(codecId);
        if (codec == null)
            reason = $"Default codec {avcodec_get_name(codecId)} does not have any available encoders";
        return codec;
    }

    /// <summary>
    /// Converts a standard C# dictionary to an AVDictionary.
    /// </summary>
    /// <param name="dict">the dictionary to convert</param>
    /// <returns>the AVDictionary</returns>
    /// <exception cref="AVException">If any FFmpeg functions fail</exception>
    public static AVDictionary* ToAVDictionary(IDictionary<string, string>? dict)
    {
        if (dict == null)
            return null;
        
        AVDictionary* res = null;
        foreach ((string key, string value) in dict)
        {
            int err;
            if ((err = av_dict_set(&res, key, value, 0)) < 0)
                throw new AVException(err);
        }
        return res;
    }
    
    /// <summary>
    /// (Re)allocates buffers for a video frame.
    /// </summary>
    /// <param name="frame">The frame</param>
    /// <param name="width">Desired width</param>
    /// <param name="height">Desired height</param>
    /// <param name="pixFmt">Desired pixel format</param>
    /// <param name="pack">If true, row data is stored continuously. This may affect performance of subsequent operations.</param>
    /// <returns>true if the frame was reallocated, false otherwise</returns>
    /// <exception cref="AVException">If any FFmpeg functions fail</exception>
    public static bool AllocVideoFrame(AVFrame* frame, int width, int height, AVPixelFormat pixFmt, bool pack = false)
    {
        if (av_frame_is_writable(frame) != 0 && frame->width == width && frame->height == height && (AVPixelFormat) frame->format == pixFmt)
            return false;
        
        if (frame->buf[0] != null)
        {
            av_frame_unref(frame);
        }

        frame->width = width;
        frame->height = height;
        frame->format = (int) pixFmt;

        int err;
        if ((err = av_frame_get_buffer(frame, pack ? 1 : 0)) < 0)
            throw new AVException(err);

        return true;
    }

    /// <summary>
    /// Fills an AVFrame with black, given a colour range
    /// </summary>
    /// <param name="frame">The frame</param>
    /// <param name="range">The colour range</param>
    public static void FrameFillBlack(AVFrame* frame, AVColorRange range)
    {
        // I would ideally like to avoid this
        var dataCopy = new byte_ptrArray4();
        CHelpers.memcpy(&dataCopy, &frame->data, 4);

        var linesizeCopy = new long_array4();
        for (uint i = 0; i < 4; i++)
            linesizeCopy[i] = frame->linesize[i];

        av_image_fill_black(ref dataCopy, in linesizeCopy, (AVPixelFormat) frame->format, range, frame->width, frame->height);
    }

    public static bool AllocAudioFrame(AVFrame* frame, int nbSamples, in AVChannelLayout layout, AVSampleFormat sampleFmt)
    {
        fixed (AVChannelLayout* pLayout = &layout)
        {
            int err = 0;
            if (av_frame_is_writable(frame) != 0 &&
                frame->nb_samples == nbSamples &&
                (AVSampleFormat) frame->format == sampleFmt &&
                (err = av_channel_layout_compare(&frame->ch_layout, pLayout)) == 0)
                return true;

            // just in case av_channel_layout_compare throws
            if (err < 0)
                throw new AVException(err);
            
            if (frame->buf[0] != null)
                av_frame_unref(frame);

            frame->nb_samples = nbSamples;
            frame->ch_layout = layout;
            frame->format = (int) sampleFmt;
            
            if ((err = av_frame_get_buffer(frame, 0)) < 0)
                throw new AVException(err);

            return true;
        }
    }

    /// <summary>
    /// Convenience method for setting up a SwsContext.
    /// </summary>
    /// <param name="sws">the SwsContext</param>
    /// <param name="src">the source frame</param>
    /// <param name="dst">the destination frame</param>
    /// <param name="flags">Any SWS_* flag to use</param>
    public static void SwsSetupFrames(ref SwsContext* sws, AVFrame* src, AVFrame* dst, int flags = SWS_AREA)
    {
        sws = sws_getCachedContext(
            sws, src->width, src->height, (AVPixelFormat) src->format, 
            dst->width, dst->height, (AVPixelFormat) dst->format, flags, 
            null, null, null
        );
    }

    /// <summary>
    /// Convenience method for setting up a SwrContext.
    /// </summary>
    /// <param name="swr">the SwrContext</param>
    /// <param name="codecCtx">the codec context</param>
    /// <param name="rate">the input sample rate</param>
    /// <exception cref="AVException">If any FFmpeg error occurs</exception>
    public static void SwrSetupInput(ref SwrContext* swr, AVCodecContext* codecCtx, int rate)
    {
        int err;
        fixed (SwrContext** pSwr = &swr)
        fixed (AVChannelLayout* pStereo = &AV_STEREO_CHANNEL_LAYOUT)
        {
            err = swr_alloc_set_opts2(pSwr,
                &codecCtx->ch_layout, codecCtx->sample_fmt, codecCtx->sample_rate,
                pStereo, AVSampleFormat.AV_SAMPLE_FMT_S16, rate,
                0, null
            );
            if (err < 0)
                throw new AVException(err);

            if ((err = swr_init(swr)) < 0)
                throw new AVException(err);
        }
    }

    /// <summary>
    /// Sets up the <paramref name="data"/> and <paramref name="linesize"/> to point to the same
    /// data, but vertically flipped.
    /// </summary>
    /// <param name="frame">The source frame</param>
    /// <param name="data">The array of data pointers. Must have 8 elements.</param>
    /// <param name="linesize">The array of line-size pointers. Must have 8 elements.</param>
    public static void SetupVFlipPointers(AVFrame* frame, out byte_ptrArray8 data, out int_array8 linesize)
    {
        data = new byte_ptrArray8();
        linesize = new int_array8();
        for (uint i = 0; i < 4; i++)
        {
            data[i] = frame->data[i] + frame->linesize[i] * (frame->height - 1);
            linesize[i] = -frame->linesize[i];
        }
    }

    /// <summary>
    /// Copies audio samples from a Mupen-owned buffer to an <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="src">The source buffer</param>
    /// <param name="dst">The destination buffer</param>
    /// <param name="len">The length</param>
    public static void CopySamples(void* src, AVFrame* dst, int len)
    {
        if (!BitConverter.IsLittleEndian)
        {
            // We can directly memcpy on big-endian systems
            CHelpers.memcpy(src, dst, (ulong) len * 4);
        }
        else
        {
            // we need to byteswap on little-endian systems due to the way Mupen organizes samples
            var pSrc = (uint*) src;
            var pDst = (uint*) dst->data[0];
            for (int i = 0; i < len; i++)
            {
                // rotr(u32 x, 16) == (x >> 16) | (x << 16)
                pDst[i] = uint.RotateRight(pSrc[i], 16);
            }
        }
    }
}