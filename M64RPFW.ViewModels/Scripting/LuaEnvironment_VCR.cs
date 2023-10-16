using M64RPFW.Models.Media.Encoder;
using M64RPFW.ViewModels.Extensions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

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
    private void FFmpeg_StartEncode(string path, string mimeType, LuaTable config)
    {
        if (EmulatorViewModel.Instance == null)
            return;
        var emu = EmulatorViewModel.Instance;

        FFmpegConfig ffConfig = new();

        _lua.IterateStringDict((LuaTable) config["format"], (key, value) =>
        {
            ffConfig.FormatOptions[key] = value;
        });
        _lua.IterateStringDict((LuaTable) config["audio"], (key, value) =>
        {
            ffConfig.AudioOptions[key] = value;
        });
        _lua.IterateStringDict((LuaTable) config["video"], (key, value) =>
        {
            ffConfig.VideoOptions[key] = value;
        });
        
        
        emu.StartEncoderCommand.ExecuteIfPossible();
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