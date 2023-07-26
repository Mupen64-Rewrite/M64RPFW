using FFmpeg.AutoGen;

namespace M64RPFW.Models.Media;

public unsafe delegate void ReadScreenCallback(void* data);

public unsafe interface IVideoEncoder : IDisposable
{
    void ConsumeVideo(int width, int height, ReadScreenCallback readScreen);
    void SetAudioSampleRate(int sampleRate);
    void ConsumeAudio(void* data, ulong len);
}