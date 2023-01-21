using System.Diagnostics;
using M64RPFW.Models.Emulation.API;
using M64RPFW.Services;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation;

public class Emulator
{
    private readonly IFilesService _filesService;

    private Thread? _emulatorThread;
    private DateTime _emulatorThreadBeginTime, _emulatorThreadEndTime;
    private bool _internalTermination;

    private PlayModes _playMode = PlayModes.Stopped;

    public Emulator(IFilesService filesService)
    {
        this._filesService = filesService;
        Api = new Mupen64PlusApi(_filesService);
    }

    public PlayModes PlayMode
    {
        get => _playMode;
        set
        {
            _playMode = value;
            Api.SetPlayMode(_playMode);
            PlayModeChanged?.Invoke();
        }
    }

    public Mupen64PlusApi Api { get; }

    public event Action? PlayModeChanged;
    
    public void Start(Mupen64PlusLaunchParameters mupen64PlusLaunchParameters)
    {
        _playMode = PlayModes.Running;
        PlayModeChanged?.Invoke();

        _emulatorThread = new Thread(() =>
        {
            EmulatorThreadProc(mupen64PlusLaunchParameters);
        })
        {
            Name = "tEmulatorThread"
        };

        _emulatorThread.Start();
        
        _emulatorThreadBeginTime = DateTime.Now;
    }

    private void EmulatorThreadProc(Mupen64PlusLaunchParameters mupen64PlusLaunchParameters)
    {
        Api.Launch(mupen64PlusLaunchParameters);

        _emulatorThreadEndTime = DateTime.Now;

        if (!_internalTermination)
        {
            // thread was killed by m64p, not by us
            _playMode = PlayModes.Stopped;
            PlayModeChanged?.Invoke();
        }

        Debug.Print($"Emulator thread exited after {_emulatorThreadEndTime - _emulatorThreadBeginTime}");
    }

    public void Stop()
    {
        _internalTermination = true;

        Api.Dispose();
        _emulatorThread.Join();

        _playMode = PlayModes.Stopped;
        PlayModeChanged?.Invoke();

        Debug.Print("Emulator stopped by host");
        _internalTermination = false;
    }

    public void Reset()
    {
        Api.Reset(true);
    }
}