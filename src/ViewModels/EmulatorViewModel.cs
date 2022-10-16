using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation.Core.API;
using M64RPFW.Models.Helpers;
using M64RPFW.src.Models.Emulation.Core.API;
using M64RPFW.UI.Other.Platform;
using M64RPFW.UI.ViewModels.Interaction;
using M64RPFW.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using static M64RPFW.Models.Emulation.Provider.GameInfoProvider;

namespace M64RPFW.UI.ViewModels
{
    public partial class EmulatorViewModel : ObservableObject
    {
        // DI - used for adding new entries
        private IRecentRomsProvider recentROMsInterface;

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
                //ICommandHelper.NotifyCanExecuteChanged(CloseROMCommand, ResetROMCommand, FrameAdvanceCommand, TogglePauseCommand);
                Mupen64PlusAPI.Instance.SetPlayMode(isResumed ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused);
            }
        }



        [RelayCommand]
        private void LoadROM()
        {
            (string ReturnedPath, bool Cancelled) status = WindowsShellWrapper.OpenFileDialogPrompt(ValidROMFileExtensions);
            string path = status.ReturnedPath;
            if (status.Cancelled) return;
            LoadROMFromPath(path);
        }

        [RelayCommand]
        private void LoadROMFromPath(string path)
        {
            if (!CheckDependencyValidity()) return;

            ROMViewModel rom = new(path);

            if (!new ROMViewModel(path).IsValid)
            {
                DialogHelper.ShowErrorDialog(Properties.Resources.InvalidROMError);
                return;
            }

            recentROMsInterface.AddRecentROM(rom);  // add recent rom here, but not in LoadROMFromPath because the latter is called by recent rom module itself

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
            bool coreLibraryExists = !DialogHelper.ShowErrorDialogIf(Properties.Resources.CoreLibraryNotFound, !File.Exists(Properties.Settings.Default.CoreLibraryPath));
            bool videoPluginExists = File.Exists(Properties.Settings.Default.VideoPluginPath);
            bool audioPluginExists = File.Exists(Properties.Settings.Default.AudioPluginPath);
            bool inputPluginExists = File.Exists(Properties.Settings.Default.InputPluginPath);
            bool rspPluginExists = File.Exists(Properties.Settings.Default.RSPPluginPath);
            if (!videoPluginExists) missingPlugins.Add(Properties.Resources.Video);
            if (!audioPluginExists) missingPlugins.Add(Properties.Resources.Audio);
            if (!inputPluginExists) missingPlugins.Add(Properties.Resources.Input);
            if (!rspPluginExists) missingPlugins.Add(Properties.Resources.RSP);
            DialogHelper.ShowErrorDialogIf(string.Format(Properties.Resources.PluginNotFoundSeries, string.Join(", ", missingPlugins)), !videoPluginExists || !audioPluginExists || !inputPluginExists || !rspPluginExists);
            return coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists && rspPluginExists;
        }

        public EmulatorViewModel(IRecentRomsProvider recentROMsViewModel)
        {
            recentROMsInterface = recentROMsViewModel;
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

            Debug.Print($"Emulator thread exited after {emulatorThreadEndTime - emulatorThreadBeginTime}");
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
