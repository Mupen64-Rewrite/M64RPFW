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

    public Mupen64PlusApi? Api { get; private set; }

    public event Action? PlayModeChanged;

    public void Start(Mupen64PlusLaunchParameters mupen64PlusLaunchParameters)
    {
        Api = new Mupen64PlusApi(_filesService);

        _playMode = PlayModes.Running;
        PlayModeChanged?.Invoke();

        _emulatorThread = new Thread(EmulatorThreadProc)
        {
            Name = "tEmulatorThread"
        };

        _emulatorThread.Start(mupen64PlusLaunchParameters);

        _emulatorThreadBeginTime = DateTime.Now;
    }

    private void EmulatorThreadProc(object @params)
    {
        var mupen64PlusLaunchParameters = (Mupen64PlusLaunchParameters)@params;

        Api.Launch(mupen64PlusLaunchParameters);

        _emulatorThreadEndTime = DateTime.Now;

        if (!_internalTermination)
        {
            // thread killed by m64p
            // we can dispatch onto UI thread only in this case, otherwise we have a deadlock!
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

        Debug.Print("Stopped");
        _internalTermination = false;
    }

    public void Reset()
    {
        Api.Reset(true);
    }
}