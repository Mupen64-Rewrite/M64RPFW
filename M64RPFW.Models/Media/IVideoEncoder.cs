using FFmpeg.AutoGen;
using M64RPFW.Services;

namespace M64RPFW.Models.Media;

public unsafe delegate void ReadScreenCallback(void* data);

public unsafe interface IVideoEncoder : IDisposable
{
    void ConsumeVideo(int width, int height, ICaptureService readScreen);
    void SetAudioSampleRate(int sampleRate);
    void ConsumeAudio(void* data, ulong len);
}