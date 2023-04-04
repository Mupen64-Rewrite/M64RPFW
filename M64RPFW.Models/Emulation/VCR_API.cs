using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    public static uint VCR_GetCurFrame()
    {
        return _vcrGetCurFrame();
    }

    public static void VCR_SetKeys(Buttons keys, uint port)
    {
        _vcrSetKeys(keys, port);
    }

    public static Buttons VCR_GetKeys(uint port)
    {
        _vcrGetKeys(out var keys, port);
        return keys;
    }

    public static bool VCR_IsPlaying => _vcrIsPlaying();

    public static bool VCR_IsReadOnly
    {
        get => _vcrIsReadOnly();
        set => _vcrSetReadOnly(value);
    }

    public static void VCR_StartRecording(string path, string author = "", string desc = "", VCRStartType startType = VCRStartType.FromReset)
    {
        Error err = _vcrStartRecording(path, author, desc, startType);
        ThrowForError(err);
    }

    public static void VCR_StartMovie(string path)
    {
        Error err = _vcrStartMovie(path);
        ThrowForError(err);
    }

    public static void VCR_StopMovie()
    {
        _vcrStopMovie(false);
    }

    public static void VCR_RestartMovie()
    {
        _vcrStopMovie(true);
    }
}