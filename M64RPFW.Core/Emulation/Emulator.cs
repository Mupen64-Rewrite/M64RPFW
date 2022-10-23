using M64RPFW.Models.Emulation.API;
using System.Diagnostics;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation
{
    public class Emulator
    {
        public event Action? IsRunningChanged;
        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning; set
            {
                isRunning = value;
                IsRunningChanged?.Invoke();
            }
        }

        public event Action? PlayModeChanged;
        private PlayModes playMode;
        public PlayModes PlayMode
        {
            get => playMode; set
            {
                playMode = value;
                API.SetPlayMode(playMode);
                PlayModeChanged?.Invoke();
            }
        }

        public Mupen64PlusAPI API { get; private set; }
        private Thread emulatorThread;
        private DateTime emulatorThreadBeginTime, emulatorThreadEndTime;


        public void Start(Mupen64PlusLaunchParameters mupen64PlusLaunchParameters)
        {
            API = new();

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

            IsRunning = true;

            API.Launch(mupen64PlusLaunchParameters);

            IsRunning = false;

            emulatorThreadEndTime = DateTime.Now;

            Debug.Print($"Emulator thread exited after {emulatorThreadEndTime - emulatorThreadBeginTime}");
        }

        public void Stop()
        {
            API.Dispose();
            Debug.Print("Finished queueing exit");
        }

        public void Reset()
        {
            API.Reset(true);
        }
    }
}
