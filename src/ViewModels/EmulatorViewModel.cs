using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFWAvalonia.Models.Emulation.Core.API;
using M64RPFWAvalonia.Properties;
using M64RPFWAvalonia.src.Models.Interaction.FileDialog;
using M64RPFWAvalonia.src.Models.Interaction.Interfaces;
using M64RPFWAvalonia.src.ViewModels.Interfaces;
using M64RPFWAvalonia.ViewModels;
using M64RPFWAvalonia.ViewModels.Interfaces;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace M64RPFWAvalonia.UI.ViewModels
{
    public partial class EmulatorViewModel : ObservableObject
    {
        // DI used for adding new entries
        private IRecentRomsProvider recentRomsProvider;
        // DI used for file dialog parent window
        private IGetVisualRoot getVisualRoot;
        // DI used for drawing to skia canvas
        private ISkiaCanvasProvider skiaCanvasProvider;

        public SavestatesViewModel SavestatesViewModel { get; protected set; }

        private SKBitmap? frameBuffer;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CloseROMCommand), 
            nameof(ResetROMCommand), 
            nameof(FrameAdvanceCommand), 
            nameof(TogglePauseCommand),
            nameof(SaveStateCommand),
            nameof(LoadStateCommand)
        )]

        private bool isRunning;

        private bool isResumed = true;
        public bool IsResumed
        {
            get => isResumed;
            set
            {
                if (Mupen64PlusAPI.api == null || !Mupen64PlusAPI.api.emulator_running) return;
                SetProperty(ref isResumed, value);
                Mupen64PlusAPI.api.SetPlayMode(isResumed ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused);
            }
        }



        [RelayCommand]
        private void LoadROM()
        {
            (string ReturnedPath, bool Cancelled) status = new FileDialog(getVisualRoot).OpenDialog(FileDialog.ROMFilter);
            string path = status.ReturnedPath;
            if (status.Cancelled) return;
            recentRomsProvider.AddRecentROM(new(path));  // add recent rom here, but not in LoadROMFromPath because the latter is called by recent rom module itself
            LoadROMFromPath(path);
        }

        [RelayCommand]
        private void LoadROMFromPath(string path)
        {
            if (!CheckDependencyValidity()) return;
            if (!new ROMViewModel(path).IsValid)
            {
                return;
            }
            if (IsRunning)
            {
                Stop();
            }
            Start(File.ReadAllBytes(path));
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void CloseROM()
        {
            Stop();
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void ResetROM()
        {
            Reset();
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void FrameAdvance()
        {
            if (Settings.Default.PauseOnFrameAdvance)
                IsResumed = false;

            Mupen64PlusAPI.api.FrameAdvance();
        }
        private void SetFrameBuffer(int[] data, int width, int height)
        {
            if (frameBuffer == null)
            {
                frameBuffer = new(new(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
            }

            unsafe
            {
                fixed (int* pArray = data)
                {
                    IntPtr ptr = new(pArray);
                    SKBitmap bmp = new(new(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
                    bmp.SetPixels((IntPtr)pArray);
                    skiaCanvasProvider.Draw(SKImage.FromBitmap(bmp), new(bmp.Width, bmp.Height));
                }
            }
        }

        private void OnNewFrame()
        {

            if (!Mupen64PlusAPI.api.IsFrameBufferReady)
            {
                Debug.Print("Framebuffer is not yet ready");

                // it works yeah
                //var hallo = new int[800*600];
                //Array.Fill<int>(hallo, new Random().Next(0, int.MaxValue))
                //SetFrameBuffer(hallo, 800, 600);

                return;
            }

            SetFrameBuffer(Mupen64PlusAPI.api.FrameBuffer, Mupen64PlusAPI.api.BufferWidth, Mupen64PlusAPI.api.BufferHeight);
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void TogglePause()
        {
            IsResumed ^= true;
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void SaveState()
        {
            Mupen64PlusAPI.api.SaveState(SavestatesViewModel.SaveStateSlot);
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void LoadState()
        {
            Mupen64PlusAPI.api.LoadState(SavestatesViewModel.SaveStateSlot);
        }

        private bool CheckDependencyValidity()
        {
            // Oh yeah this doesnt suck at all
            List<string> missingPlugins = new();
            bool coreLibraryExists = File.Exists(Settings.Default.CoreLibraryPath);
            bool videoPluginExists = File.Exists(Settings.Default.VideoPluginPath);
            bool audioPluginExists = File.Exists(Settings.Default.AudioPluginPath);
            bool inputPluginExists = File.Exists(Settings.Default.InputPluginPath);
            bool rspPluginExists = File.Exists(Settings.Default.RSPPluginPath);
            if (!videoPluginExists) missingPlugins.Add(Resources.ResourceManager.GetString("Video"));
            if (!audioPluginExists) missingPlugins.Add(Resources.ResourceManager.GetString("Audio"));
            if (!inputPluginExists) missingPlugins.Add(Resources.ResourceManager.GetString("Input"));
            if (!rspPluginExists) missingPlugins.Add(Resources.ResourceManager.GetString("RSP"));
            //var errorStr = string.Format(Resources.PluginNotFoundSeries, string.Join(", ", missingPlugins)), !videoPluginExists || !audioPluginExists || !inputPluginExists || !rspPluginExists);
            return coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists && rspPluginExists;
        }

        public EmulatorViewModel(IRecentRomsProvider recentRomsProvider, IGetVisualRoot getVisualRoot, ISkiaCanvasProvider skiaCanvasProvider)
        {
            this.recentRomsProvider = recentRomsProvider;
            this.getVisualRoot = getVisualRoot;
            this.skiaCanvasProvider = skiaCanvasProvider;
            SavestatesViewModel = new();
        }

        #region Emulation

        private Thread emulatorThread;
        private DateTime emulatorThreadBeginTime, emulatorThreadEndTime;
        private CancellationTokenSource emulatorCancellationTokenSource;

        private void Start(byte[] romBuffer)
        {
            emulatorCancellationTokenSource = new();
            
            emulatorThread = new(new ParameterizedThreadStart(EmulatorThreadProc))
            {
                Name = "tEmulatorThread"
            };
            emulatorThread.Start((romBuffer, emulatorCancellationTokenSource));
            emulatorThreadBeginTime = DateTime.Now;
        }

        private void EmulatorThreadProc(object data)
        {
            var tuple = ((byte[] romBuffer, CancellationTokenSource emulatorCancellationTokenSource))data;

            PlayProcess(tuple.romBuffer, // Unbox object to original type
                tuple.emulatorCancellationTokenSource,
                Properties.Settings.Default);

            // ...

            emulatorThreadEndTime = DateTime.Now;
            Debug.Print($"Emulator thread exited after {(emulatorThreadEndTime - emulatorThreadBeginTime)}");
        }

        private void Stop()
        {
            emulatorCancellationTokenSource.Cancel();
            Mupen64PlusAPI.api.Dispose();
            emulatorThread.Join();
            // just in case
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        public void Reset()
        {
            Mupen64PlusAPI.api.Reset(true);
        }

        private void PlayProcess(byte[] romBuffer, CancellationTokenSource cancellationTokenSource, Settings settings)
        {
            Mupen64PlusAPI.api = new(null);
            int frame = 0;

            Mupen64PlusAPI.api.FrameFinished += delegate
            {
                OnNewFrame();
            };

            Dispatcher.UIThread.Post(() =>
            {
                IsRunning = true;
            }, DispatcherPriority.Normal);



            Mupen64PlusAPI.api.Launch(
                    romBuffer,
                    cancellationTokenSource.Token,
                    settings
            );

            // let the emu thread get nuked by gc and windows, do not call anything after emu stop on emu thread!

            Dispatcher.UIThread.Post(() =>
            {
                IsRunning = false;
            }, DispatcherPriority.Normal);

        }


        #endregion

    }
}
