using M64RPFW.Models.Media.Encoder;
using M64RPFW.ViewModels.Extensions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("movie.playmovie")]
    private void Movie_PlayMovie(string filename)
    {
        if (EmulatorViewModel.Instance == null)
            return;
        var emu = EmulatorViewModel.Instance;

        emu.StartMovieWithFileCommand.ExecuteIfPossible(filename);

    }

    [LibFunction("movie.stopmovie")]
    private void Movie_StopMovie()
    {

        if (EmulatorViewModel.Instance == null)
            return;
        var emu = EmulatorViewModel.Instance;

        emu.StopMovieCommand.ExecuteIfPossible();
    }

    [LibFunction("ffmpeg.start_encode")]
    private void FFmpeg_StartEncode(string path, string? mimeType, LuaTable? config)
    {
        if (EmulatorViewModel.Instance == null)
            return;
        var emu = EmulatorViewModel.Instance;

        FFmpegConfig? ffConfig = null;
        if (config != null)
        {
            ffConfig = new FFmpegConfig();
            if (config["format"] is LuaTable formatTable)
            {
                _lua.IterateStringDict(formatTable, (key, value) =>
                {
                    ffConfig.FormatOptions[key] = value;
                });
            }
            if (config["audio"] is LuaTable audioTable)
            {
                _lua.IterateStringDict(audioTable, (key, value) =>
                {
                    ffConfig.AudioOptions[key] = value;
                });
            }
            if (config["video"] is LuaTable videoTable)
            {
                _lua.IterateStringDict(videoTable, (key, value) =>
                {
                    ffConfig.VideoOptions[key] = value;
                });
            }
            if (config["audio_codec"] is string audioCodec)
            {
                ffConfig.AudioCodec = audioCodec;
            }
            if (config["video_codec"] is string videoCodec)
            {
                ffConfig.VideoCodec = videoCodec;
            }
        }
        
        emu.StartEncoderWithFileCommand.ExecuteIfPossible((path, mimeType, ffConfig));
    }

    [LibFunction("ffmpeg.stop_encode")]
    private void FFmpeg_StopEncode()
    {
        
        if (EmulatorViewModel.Instance == null)
            return;
        var emu = EmulatorViewModel.Instance;
        
        emu.StopEncoderCommand.ExecuteIfPossible();
    }
}