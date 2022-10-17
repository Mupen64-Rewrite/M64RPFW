using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation.Core.API;
using M64RPFW.Models.Helpers;
using M64RPFW.src.Containers;
using M64RPFW.src.Interfaces;
using M64RPFW.src.Models.Emulation.Core.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace M64RPFW.UI.ViewModels
{
    internal partial class EmulatorViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CloseROMCommand), nameof(ResetROMCommand), nameof(FrameAdvanceCommand), nameof(TogglePauseCommand))]
        private bool isRunning;

        private bool isResumed = true;
        public bool IsResumed
        {
            get => isResumed;
            set
            {
                if (Mupen64PlusAPI.Instance == null || !Mupen64PlusAPI.Instance.emulator_running) return;
                SetProperty(ref isResumed, value);
                Mupen64PlusAPI.Instance.SetPlayMode(isResumed ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused);
            }
        }



        [RelayCommand]
        private void LoadROM()
        {
            var (ReturnedPath, Cancelled) = generalDependencyContainer.FileDialogProvider.OpenFileDialogPrompt(generalDependencyContainer.RomFileExtensionsConfigurationProvider.ROMFileExtensionsConfiguration.FileExtensions);
            if (Cancelled) return;
            LoadROMFromPath(ReturnedPath);
        }

        [RelayCommand]
        private void LoadROMFromPath(string path)
        {
            if (!CheckDependencyValidity()) return;

            ROMViewModel rom = new(path);

            if (!new ROMViewModel(path).IsValid)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(Properties.Resources.InvalidROMError);
                return;
            }

            generalDependencyContainer.RecentRomsProvider.AddRecentROM(rom);  // add recent rom here, but not in LoadROMFromPath because the latter is called by recent rom module itself

            if (IsRunning)
            {
                Stop();
            }
            Start(path);
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
            if (Properties.Settings.Default.PauseOnFrameAdvance)
                IsResumed = false;
            Mupen64PlusAPI.Instance.FrameAdvance();
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void TogglePause()
        {
            Debug.Print("A");
            IsResumed ^= true;
        }

        private bool CheckDependencyValidity()
        {
            // Oh yeah this doesnt suck at all
            List<string> missingPlugins = new();

            bool coreLibraryExists = File.Exists(Properties.Settings.Default.CoreLibraryPath);

            if (!coreLibraryExists)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(Properties.Resources.CoreLibraryNotFound);
            }

            bool videoPluginExists = File.Exists(Properties.Settings.Default.VideoPluginPath);
            bool audioPluginExists = File.Exists(Properties.Settings.Default.AudioPluginPath);
            bool inputPluginExists = File.Exists(Properties.Settings.Default.InputPluginPath);
            bool rspPluginExists = File.Exists(Properties.Settings.Default.RSPPluginPath);
            if (!videoPluginExists) missingPlugins.Add(Properties.Resources.Video);
            if (!audioPluginExists) missingPlugins.Add(Properties.Resources.Audio);
            if (!inputPluginExists) missingPlugins.Add(Properties.Resources.Input);
            if (!rspPluginExists) missingPlugins.Add(Properties.Resources.RSP);

            if (!videoPluginExists || !audioPluginExists || !inputPluginExists || !rspPluginExists)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(string.Format(Properties.Resources.PluginNotFoundSeries, string.Join(", ", missingPlugins)));
            }

            return coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists && rspPluginExists;
        }

        internal EmulatorViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;
        }

        #region Emulation

        private Thread emulatorThread;
        private DateTime emulatorThreadBeginTime, emulatorThreadEndTime;

        private void Start(string romPath)
        {
            Size windowSize = new(800, 600);
            emulatorThread = new(new ParameterizedThreadStart(EmulatorThreadProc))
            {
                Name = "tEmulatorThread"
            };

            Mupen64PlusConfig config = new();

            config.CoreType.Value = Properties.Settings.Default.CoreType;
            config.NoCompiledJump.Value = !Properties.Settings.Default.CompiledJump;
            config.DisableExtraMemory.Value = !Properties.Settings.Default.ExtraMemory;
            config.DelaySpecialInterrupt.Value = !Properties.Settings.Default.DelaySpecialInterrupt;
            config.CyclesPerOp.Value = Properties.Settings.Default.CyclesPerOp;
            config.DisableSpecialRecompilation.Value = !Properties.Settings.Default.SpecialRecompilation;
            config.RandomizeInterrupt.Value = Properties.Settings.Default.RandomizeInterrupt;

            config.ScreenWidth.Value = 800;
            config.ScreenHeight.Value = 600;
            config.VerticalSynchronization.Value = Properties.Settings.Default.VerticalSynchronization;
            config.OnScreenDisplay.Value = Properties.Settings.Default.OnScreenDisplay;

            emulatorThread.Start(new Mupen64PlusLaunchParameters(File.ReadAllBytes(romPath),
                                                                 config,
                                                                 Properties.Settings.Default.DefaultSlot,
                                                                 Properties.Settings.Default.CoreLibraryPath,
                                                                 Properties.Settings.Default.VideoPluginPath,
                                                                 Properties.Settings.Default.AudioPluginPath,
                                                                 Properties.Settings.Default.InputPluginPath,
                                                                 Properties.Settings.Default.RSPPluginPath));

            emulatorThreadBeginTime = DateTime.Now;
        }

        private void EmulatorThreadProc(object @params)
        {
            Mupen64PlusLaunchParameters mupen64PlusLaunchParameters = (Mupen64PlusLaunchParameters)@params;

            Mupen64PlusAPI.Instance = new();

            int frame = 0;
            Mupen64PlusAPI.Instance.FrameFinished += delegate
            {
                Debug.Print($"Frame {frame++}");
            };

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                IsRunning = true;
            }));


            Mupen64PlusAPI.Instance.Launch(mupen64PlusLaunchParameters);

            Mupen64PlusAPI.Instance.Dispose();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                IsRunning = false;
            }));


            emulatorThreadEndTime = DateTime.Now;

            Debug.Print($"Emulator thread exited after {(emulatorThreadEndTime - emulatorThreadBeginTime)}");
        }

        private void Stop()
        {
            Mupen64PlusAPI.Instance.Dispose();
            emulatorThread.Join();
        }

        public void Reset()
        {
            Mupen64PlusAPI.Instance.Reset(true);
        }

        #endregion

    }
}
