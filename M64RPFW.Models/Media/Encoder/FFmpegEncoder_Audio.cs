using FFmpeg.AutoGen;
using M64RPFW.Models.Media.Helpers;
using static FFmpeg.AutoGen.ffmpeg;

namespace M64RPFW.Models.Media.Encoder;

public unsafe partial class FFmpegEncoder
{
    private class AudioStream : FFmpegEncodeStream
    {
        
        private AVFrame* _frame1;
        private AVFrame* _frame2;
        private SwrContext* _swr;
        private long _frameSize;
        
        private SemaphoreSlim _sem;
        

        public AudioStream(AVFormatContext* fmtCtx, AVCodec* codec, IDictionary<string, string>? config = null) : base(fmtCtx, codec, StreamConfig, config)
        {
            _sem = new SemaphoreSlim(0, 1);
            
            _frame1 = av_frame_alloc();
            _frame2 = av_frame_alloc();
            if (_frame1 == null || _frame2 == null)
            {
                throw new Exception("FFmpeg frame allocation failed");
            }

            _frameSize = _codecCtx->frame_size != 0 ? _codecCtx->frame_size : 4096;

            _swr = null;
        }

        private static void StreamConfig(AVCodecContext* codecCtx, AVStream* stream, AVCodec* codec)
        {
            codecCtx->ch_layout = AVHelpers.AV_STEREO_CHANNEL_LAYOUT;
            codecCtx->sample_rate = 48000;
            if (codec->sample_fmts != null) {
                AVSampleFormat fmt = codec->sample_fmts[0];
                for (AVSampleFormat* i = codec->sample_fmts; *i != (AVSampleFormat) (-1); i++) {
                    if (*i == AVSampleFormat.AV_SAMPLE_FMT_S16) {
                        fmt = *i;
                        break;
                    }
                    if (*i == AVSampleFormat.AV_SAMPLE_FMT_S16P)
                        fmt = *i;
                }
                codecCtx->sample_fmt = fmt;
            }
            else
                codecCtx->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;
            codecCtx->bit_rate = 196000;
            // time_base = 1/sample_rate
            codecCtx->time_base = new AVRational
                {num = 1, den = codecCtx->sample_rate};
            stream->time_base    = codecCtx->time_base;

            // no idea why this is here, but I'm all for it
            codecCtx->strict_std_compliance = FF_COMPLIANCE_EXPERIMENTAL;
        }

        protected override void ReleaseUnmanagedResources()
        {
            base.ReleaseUnmanagedResources();
            AVHelpers.Dispose(ref _frame1);
            AVHelpers.Dispose(ref _frame2);
            AVHelpers.Dispose(ref _swr);
        }

        public void SetSampleRate(int rate)
        {
            int err;
            
            _sem.Wait();
            try
            {
                // this might drop samples, but I doubt that'll ever happen
                if (_swr != null && swr_is_initialized(_swr) != 0)
                    swr_close(_swr);
                
                AVHelpers.SwrSetupInput(ref _swr, _codecCtx, rate);
            }
            finally
            {
                _sem.Release();
            }
        }

        public void ConsumeSamples(void* samples, int len)
        {
            _sem.Wait();

            int iLen = len / 4;

            try
            {
                AVHelpers.AllocAudioFrame(_frame1, iLen, in AVHelpers.AV_STEREO_CHANNEL_LAYOUT, AVSampleFormat.AV_SAMPLE_FMT_S16);
                AVHelpers.CopySamples(samples, _frame1, iLen);
            }
            catch
            {
                _sem.Release();
                throw;
            }

            Task.Run(ConsumeSamplesCore);
        }

        private void ConsumeSamplesCore()
        {
            try
            {
                int err;
                if ((err = swr_convert(_swr, null, 0, (byte**) &_frame1->data, _frame1->nb_samples)) < 0)
                    throw new AVException(err);

                while (swr_get_out_samples(_swr, 0) >= _frameSize)
                {
                    AVHelpers.AllocAudioFrame(_frame2, (int) _frameSize, in _codecCtx->ch_layout, _codecCtx->sample_fmt);
                    if ((err = swr_convert(_swr, (byte**) &_frame2->data, _frame2->nb_samples, null, 0)) < 0)
                        throw new AVException(err);

                    _frame2->pts = _pts;
                    _pts += _frame2->nb_samples;
                    
                    EncodeFrame(_frame2);
                }
            }
            finally
            {
                _sem.Release();
            }
        }
        public void Flush()
        {
            try
            {
                int err;
                // drain last bytes from resampler
                int nbSamples = swr_get_out_samples(_swr, 0);

                AVHelpers.AllocAudioFrame(_frame2, nbSamples, in _codecCtx->ch_layout, _codecCtx->sample_fmt);
                
                if ((err = swr_convert(_swr, (byte**) &_frame2->data, _frame2->nb_samples, null, 0)) < 0)
                    throw new AVException(err);
                
                _frame2->pts = _pts;
                _pts += _frame2->nb_samples;
                
                EncodeFrame(_frame2);
                EncodeFrame(null);
            }
            finally
            {
                _sem.Release();
            }
        }
    }

    
}