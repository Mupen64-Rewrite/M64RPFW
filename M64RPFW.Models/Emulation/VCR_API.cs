using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    /// <summary>
    /// Gets the index of the current input frame.
    /// </summary>
    /// <returns>the index of the current input frame.</returns>
    public static uint VCR_GetCurFrame()
    {
        return _vcrGetCurFrame();
    }
    
    /// <summary>
    /// Sets the VCR input overlay for a given controller port.
    /// </summary>
    /// <param name="keys">the inputs</param>
    /// <param name="port">which controller port to write</param>
    public static void VCR_SetOverlay(Buttons keys, uint port)
    {
        _vcrSetOverlay(keys, port);
    }

    /// <summary>
    /// Gets the current VCR inputs for a given controller port.
    /// </summary>
    /// <param name="port">which controller port to read</param>
    public static Buttons VCR_GetKeys(uint port)
    {
        _vcrGetKeys(out var keys, port);
        return keys;
    }

    public static bool VCR_IsPlaying => _vcrIsPlaying();
    
    /// <summary>
    /// Property controlling the readonly state.
    /// However, its functionality doesn't exactly match Mupen64: it will immediately start copying inputs
    /// as soon as readonly is disabled. Hence why I chose to call it "Disable Writes" instead.
    /// </summary>
    public static bool VCR_DisableWrites
    {
        get => _vcrIsReadOnly();
        set => _vcrSetReadOnly(value);
    }
    
    /// <summary>
    /// Initiates VCR recording to a file.
    /// </summary>
    /// <param name="path">the M64 path to write to</param>
    /// <param name="author">a string stored as the "Authors" field</param>
    /// <param name="desc">a string stored as the "Description" field</param>
    /// <param name="startType">what to do before starting this M64</param>
    public static void VCR_StartRecording(string path, string author = "", string desc = "", VCRStartType startType = VCRStartType.Reset)
    {
        Error err = _vcrStartRecording(path, author, desc, startType);
        ThrowForError(err);
    }
    
    /// <summary>
    /// Initiates VCR playback from a file.
    /// </summary>
    /// <param name="path">the M64 path to read from</param>
    public static void VCR_StartMovie(string path)
    {
        Error err = _vcrStartMovie(path);
        ThrowForError(err);
    }
    
    /// <summary>
    /// Stops the currently playing movie.
    /// </summary>
    public static void VCR_StopMovie()
    {
        _vcrStopMovie(false);
    }
    
    /// <summary>
    /// Resets the currently playing movie.
    /// </summary>
    public static void VCR_RestartMovie()
    {
        _vcrStopMovie(true);
    }
}