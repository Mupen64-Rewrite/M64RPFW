using FFmpeg.AutoGen;

namespace M64RPFW.Models.Media.Encoder;

/// <summary>
/// Data class containing FFmpeg-related config data.
/// </summary>
public class FFmpegConfig
{
    /// <summary>
    /// Set of options passed to <see cref="ffmpeg.avformat_write_header"/>.
    /// </summary>
    public Dictionary<string, string> FormatOptions { get; } = new();
    /// <summary>
    /// Set of options passed to <see cref="ffmpeg.avcodec_open2"/> for the video codec.
    /// </summary>
    public Dictionary<string, string> VideoOptions { get; } = new();
    /// <summary>
    /// Set of options passed to <see cref="ffmpeg.avcodec_open2"/> for the audio codec.
    /// </summary>
    public Dictionary<string, string> AudioOptions { get; } = new();

    /// <summary>
    /// If set, manually selects a video codec, as opposed to using FFmpeg's default.
    /// </summary>
    public string? VideoCodec { get; set; } = null;
    /// <summary>
    /// If set, manually selects an audio codec, as opposed to using FFmpeg's default.
    /// </summary>
    public string? AudioCodec { get; set; } = null;

    /// <summary>
    /// If true, disables the audio stream entirely.
    /// </summary>
    public bool NoAudio { get; set; } = false;
}