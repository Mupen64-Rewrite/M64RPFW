using System.Text;

namespace M64RPFW.Models.Emulation;

public unsafe partial class Mupen64Plus
{
    #region Useful delegate types

    public delegate void SampleSpanAction(void* data, ulong len);

    #endregion
    private static readonly SampleCallback _sampleCallback;
    private static readonly RateChangedCallback _rateChangedCallback;

    
    public static bool Encoder_IsActive()
    {
        return _encoderIsActive();
    }

    public static void Encoder_Start(string path, string? fmt)
    {
        byte[] pathData = Encoding.ASCII.GetBytes(path + char.MinValue);
        byte[]? fmtData = fmt != null ? Encoding.ASCII.GetBytes(fmt + char.MinValue) : null;
        fixed (byte* pPathData = pathData, pFmtData = fmtData)
        {
            _encoderStart(pPathData, pFmtData);
        }
    }

    public static void Encoder_Stop(bool discard)
    {
        _encoderStop(discard);
    }

    private static void OnSample(void* data, nint length)
    {
        AudioReceived?.Invoke(data, (ulong) length);
    }

    private static void OnRateChanged(uint rate)
    {
        SampleRateChanged?.Invoke(rate);
    }

    public static event SampleSpanAction? AudioReceived;
    public static event Action<uint>? SampleRateChanged;

    public static uint GetSampleRate()
    {
        return _encoderGetSampleRate();
    }
}