using FFmpeg.AutoGen;
using M64RPFW.Models.Media.Helpers;
using static FFmpeg.AutoGen.ffmpeg;

namespace M64RPFW.Models.Media.Encoder;

public unsafe partial class FFmpegEncoder
{
    private class VideoStream : FFmpegEncodeStream
    {
        private AVFrame* _frame1;
        private AVFrame* _frame2;
        private SwsContext* _sws;
        private SemaphoreSlim _sem;

        private FFmpegEncodeStream? _syncStream;
        
        public VideoStream(AVFormatContext* fmtCtx, AVCodec* codec, FFmpegEncodeStream? syncStream, IDictionary<string, string>? config = null) : 
            base(fmtCtx, codec, StreamConfig, config)
        {
            _sem = new SemaphoreSlim(0, 1);
            _syncStream = syncStream;

            _frame1 = av_frame_alloc();
            _frame2 = av_frame_alloc();
            if (_frame1 == null || _frame2 == null)
            {
                throw new Exception("FFmpeg frame allocation failed");
            }

            _sws = null;

            // fill frame2 with black, this is required for the synchronizer.
            AVHelpers.AllocVideoFrame(_frame2, _codecCtx->width, _codecCtx->height, _codecCtx->pix_fmt);
            AVHelpers.FrameFillBlack(_frame2, _codecCtx->color_range);
        }

        private static void StreamConfig(AVCodecContext* codecCtx, AVStream* stream, AVCodec* codec)
        {
            // Setup private data
            av_opt_set(codecCtx->priv_data, "preset", "fast", 0);
            // width/height (this will come from config later)
            codecCtx->width = 640;
            codecCtx->coded_width = 640;
            codecCtx->height = 480;
            codecCtx->coded_height = 480;
            // pixel aspect ratio (they're square)
            codecCtx->sample_aspect_ratio = new AVRational
            {
                num = 1,
                den = 1
            };
            // pixel format (use YCbCr 4:2:0 if not known)
            if (codec->pix_fmts != null)
                codecCtx->pix_fmt = codec->pix_fmts[0];
            else
                codecCtx->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            // bitrate settings (reasonable defaults, I guess)
            codecCtx->bit_rate = 2_000_000;
            codecCtx->rc_buffer_size = 4_000_000;
            codecCtx->rc_max_rate = 2_500_000;
            codecCtx->rc_min_rate = 2_000_000;
            // time_base = 1/frame_rate
            codecCtx->time_base = new AVRational
            {
                num = 1,
                den = 60
            };
            stream->time_base = new AVRational
            {
                num = 1,
                den = 60
            };
        }

        public override void Dispose()
        {
            base.Dispose();
            AVHelpers.Dispose(ref _frame1);
            AVHelpers.Dispose(ref _frame2);
        }

        public void ConsumeFrame(int width, int height, ReadScreenCallback readScreen)
        {
            // This part has to run synchronously when the new frame arrives
            _sem.Wait();
            try
            {
                if (width != _codecCtx->width || height != _codecCtx->height)
                    throw new Exception("INTERNAL: width/height should match");

                AVHelpers.AllocVideoFrame(_frame1, width, height, AVPixelFormat.AV_PIX_FMT_RGB24, true);
                readScreen(_frame1->data[0]);
            }
            catch
            {
                _sem.Release();
                throw;
            }
            // The actual encoding part can run asynchronously
            Task.Run(ConsumeFrameCore);
        }

        private void ConsumeFrameCore()
        {
            try
            {
                if (_syncStream != null)
                {
                    while (av_compare_ts(_pts, _codecCtx->time_base, _syncStream._pts, _syncStream._codecCtx->time_base) < 0)
                    {
                        _frame2->pts = _pts++;
                        EncodeFrame(_frame2);
                    }
                }
                
                AVHelpers.AllocVideoFrame(_frame2, _codecCtx->width, _codecCtx->height, _codecCtx->pix_fmt);
                AVHelpers.SwsSetupFrames(ref _sws, _frame1, _frame2);
                
                // ReadScreen2 flips the image for some reason, so we have to flip the image while scaling
                int err;
                AVHelpers.SetupVFlipPointers(_frame1, out var flipData, out var flipLinesize);
                if ((err = sws_scale(_sws, flipData, flipLinesize, 0, _frame1->height, _frame2->data, _frame2->linesize)) < 0)
                    throw new AVException(err);
                
                // Set the timestamp for this frame
                _frame2->time_base = _codecCtx->time_base;
                _frame2->pts = _pts++;
                
                EncodeFrame(_frame2);
            }
            finally
            {
                _sem.Release();
            }
        }
    }
}