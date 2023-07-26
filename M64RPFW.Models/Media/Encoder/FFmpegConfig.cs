namespace M64RPFW.Models.Media.Encoder;

public class FFmpegConfig
{
    public readonly Dictionary<string, string> FormatOptions;
    public readonly Dictionary<string, string> VideoOptions;
    public readonly Dictionary<string, string> AudioOptions;

    public FFmpegConfig()
    {
        FormatOptions = new Dictionary<string, string>();
        VideoOptions = new Dictionary<string, string>();
        AudioOptions = new Dictionary<string, string>();
    }
}