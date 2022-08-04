using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using M64PRR.Models.Helpers;
using static M64RPFW.Models.Emulation.Mupen64Plus.Mupen64Plus;

namespace M64RPFW.Models.Emulation.Mupen64Plus;

public partial class Mupen64Plus
{
    // Public API
    // ========================================================
#pragma warning disable CS8618
    public Mupen64Plus(string path)
    {
        _libHandle = NativeLibrary.Load(path);

        // Resolve all needed symbols in advance. Warnings needed
        // to be disabled because that initialization takes place
        // in a function (outside the constructor).
        ResolveFrontendFunctions();

        Error err = _fnCoreStartup!(
            0x020000, null, null, IntPtr.Zero,
            null, IntPtr.Zero, null);
        ThrowForError(err);
    }
#pragma warning restore CS8618

    ~Mupen64Plus()
    {
        ThrowForError(_fnCoreShutdown());
        NativeLibrary.Free(_libHandle);
    }

    public void OpenROM(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        ROMHelper.AdaptiveByteSwap(ref bytes);

        Error err = CoreDoCommand(Command.RomOpen, 0, bytes);
        ThrowForError(err);
    }

    public void CloseROM()
    {
        Error err = CoreDoCommand(Command.RomClose, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public RomHeader GetROMHeader()
    {
        RomHeader header = new RomHeader();
        Error err = CoreDoCommand(Command.RomGetHeader, Marshal.SizeOf(header), ref header);
        ThrowForError(err);

        return header;
    }

    public RomHeader GetROMHeader(ref RomHeader header)
    {
        Error err = CoreDoCommand(Command.RomGetHeader, Marshal.SizeOf(header), ref header);
        ThrowForError(err);

        return header;
    }

    public RomSettings GetROMSettings()
    {
        RomSettings settings = new RomSettings();
        Error err = CoreDoCommand(Command.RomGetSettings, Marshal.SizeOf(settings), ref settings);
        ThrowForError(err);
        return settings;
    }

    public RomSettings GetROMSettings(ref RomSettings settings)
    {
        Error err = CoreDoCommand(Command.RomGetSettings, Marshal.SizeOf(settings), ref settings);
        ThrowForError(err);
        return settings;
    }

    public void Execute()
    {
        Error err = CoreDoCommand(Command.Execute, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Stop()
    {
        Error err = CoreDoCommand(Command.Stop, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Pause()
    {
        Error err = CoreDoCommand(Command.Pause, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Resume()
    {
        Error err = CoreDoCommand(Command.Resume, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public int CoreStateQuery(CoreParam param)
    {
        int res = 0;
        Error err = CoreDoCommand(Command.CoreStateQuery, (int) param, ref res);
        ThrowForError(err);
        return res;
    }

    public int CoreStateQuery(CoreParam param, ref int res)
    {
        Error err = CoreDoCommand(Command.CoreStateQuery, (int) param, ref res);
        ThrowForError(err);
        return res;
    }

    public void CoreStateSet(CoreParam param, int value)
    {
        Error err = CoreDoCommand(Command.CoreStateSet, (int) param, ref value);
        ThrowForError(err);
    }

    public void LoadStateFromFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        Error err = CoreDoCommand(Command.StateLoad, 0, Path.GetFullPath(path));
        ThrowForError(err);
    }

    public void LoadStateFromCurrentSlot()
    {
        Error err = CoreDoCommand(Command.StateLoad, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void SaveStateToFile(string path, SavestateType type = SavestateType.Mupen64Plus)
    {
        ArgumentNullException.ThrowIfNull(path);
        Error err = CoreDoCommand(Command.StateSave, (int) type, Path.GetFullPath(path));
        ThrowForError(err);
    }

    public void SaveStateToCurrentSlot()
    {
        Error err = CoreDoCommand(Command.StateSave, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void SetSavestateSlot(int slot)
    {
        if (slot < 0 || slot > 9)
            throw new ArgumentOutOfRangeException(nameof(slot), "Savestate slots range from 0-9 (inclusive)");
        Error err = CoreDoCommand(Command.StateSetSlot, slot, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Reset(bool hard = true)
    {
        Error err = CoreDoCommand(Command.Reset, hard? 1 : 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void AdvanceFrame()
    {
        Error err = CoreDoCommand(Command.AdvanceFrame, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void OverrideVideoExtensions(IVideoExtension obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        _vidextObject = obj;
        _vidextDelegates = new(obj);

        Error err = _fnCoreOverrideVidExt(_vidextDelegates!.AsNative());
        ThrowForError(err);
    }

    private readonly IntPtr _libHandle;
}