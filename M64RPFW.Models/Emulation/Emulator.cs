using M64RPFW.Models.Emulation.API;
using System.Diagnostics;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation
{
    public class Emulator
    {
        public event Action? PlayModeChanged;
        private PlayModes playMode = PlayModes.Stopped;
        public PlayModes PlayMode
        {
            get => playMode; set
            {
                playMode = value;
                API.SetPlayMode(playMode);
                PlayModeChanged?.Invoke();
            }
        }

        public Mupen64PlusAPI? API { get; private set; }
        private Thread? emulatorThread;
        private DateTime emulatorThreadBeginTime, emulatorThreadEndTime;


        public void Start(Mupen64PlusLaunchParameters mupen64PlusLaunchParameters)
        {
            API = new();

            playMode = PlayModes.Running;
            PlayModeChanged?.Invoke();

            emulatorThread = new(new ParameterizedThreadStart(EmulatorThreadProc))
            {
                Name = "tEmulatorThread"
            };

            emulatorThread.Start(mupen64PlusLaunchParameters);

            emulatorThreadBeginTime = DateTime.Now;
        }

        private void EmulatorThreadProc(object @params)
        {
            Mupen64PlusLaunchParameters mupen64PlusLaunchParameters = (Mupen64PlusLaunchParameters)@params;

            API.Launch(mupen64PlusLaunchParameters);

            emulatorThreadEndTime = DateTime.Now;

            Debug.Print($"Emulator thread exited after {emulatorThreadEndTime - emulatorThreadBeginTime}");
        }

        public void Stop()
        {
            API.Dispose();
            emulatorThread.Join();

            playMode = PlayModes.Stopped;
            PlayModeChanged?.Invoke();

            Debug.Print("Finished queueing exit");
        }

        public void Reset()
        {
            API.Reset(true);
        }
    }
}
