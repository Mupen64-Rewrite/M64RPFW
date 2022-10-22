using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation.Core.API;
using M64RPFW.src.Containers;
using M64RPFW.src.Models.Emulation.Core.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
                if (Mupen64PlusAPI.Instance == null || !Mupen64PlusAPI.Instance.IsEmulatorRunning) return;
                SetProperty(ref isResumed, value);
                Mupen64PlusAPI.Instance.SetPlayMode(isResumed ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused);
            }
        }



        [RelayCommand]
        private void LoadROM()
        {
            (string ReturnedPath, bool Cancelled) = generalDependencyContainer.FileDialogProvider.OpenFileDialogPrompt(generalDependencyContainer.RomFileExtensionsConfigurationProvider.ROMFileExtensionsConfiguration.FileExtensions);
            if (Cancelled) return;
            LoadROMFromPath(ReturnedPath);
        }

        [RelayCommand]
        private void LoadROMFromPath(string path)
        {
            if (!AreAllDependenciesMet()) return;

            ROMViewModel rom = new(path);

            if (!new ROMViewModel(path).IsValid)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(generalDependencyContainer.LocalizationProvider.GetString("ROMInexistent"));
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
            IsResumed ^= true;
        }

        private bool AreAllDependenciesMet()
        {
            // TODO: clean this up

            List<string> missingPlugins = new();

            bool coreLibraryExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath);

            if (!Path.GetExtension(generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath).Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                coreLibraryExists = false;
            }

            if (!coreLibraryExists)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(generalDependencyContainer.LocalizationProvider.GetString("CoreLibraryNotFound"));
            }

            bool videoPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().VideoPluginPath);
            bool audioPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().AudioPluginPath);
            bool inputPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().InputPluginPath);
            bool rspPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().RSPPluginPath);
            if (!videoPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Video"));
            if (!audioPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Audio"));
            if (!inputPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Input"));
            if (!rspPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("RSP"));

            if (!videoPluginExists || !audioPluginExists || !inputPluginExists || (!rspPluginExists && coreLibraryExists))
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
            emulatorThread = new(new ParameterizedThreadStart(EmulatorThreadProc))
            {
                Name = "tEmulatorThread"
            };

            Mupen64PlusConfig config = new(generalDependencyContainer.SettingsManager.GetSettings().CoreType, generalDependencyContainer.SettingsManager.GetSettings().ScreenWidth, generalDependencyContainer.SettingsManager.GetSettings().ScreenHeight);


            emulatorThread.Start(new Mupen64PlusLaunchParameters(File.ReadAllBytes(romPath),
                                                                 config,
                                                                 generalDependencyContainer.SettingsManager.GetSettings().DefaultSlot,
                                                                 generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath,
                                                                 generalDependencyContainer.SettingsManager.GetSettings().VideoPluginPath,
                                                                 generalDependencyContainer.SettingsManager.GetSettings().AudioPluginPath,
                                                                 generalDependencyContainer.SettingsManager.GetSettings().InputPluginPath,
                                                                 generalDependencyContainer.SettingsManager.GetSettings().RSPPluginPath));

            emulatorThreadBeginTime = DateTime.Now;
        }

        private void EmulatorThreadProc(object @params)
        {
            Mupen64PlusLaunchParameters mupen64PlusLaunchParameters = (Mupen64PlusLaunchParameters)@params;

            Mupen64PlusAPI.Create();

            int frame = 0;

            Mupen64PlusAPI.Instance.OnFrameBufferCreated += delegate
            {
                generalDependencyContainer.DrawingSurfaceProvider.Create(Mupen64PlusAPI.Instance.BufferWidth, Mupen64PlusAPI.Instance.BufferHeight);
            };
            Mupen64PlusAPI.Instance.OnFrameBufferUpdate += delegate
            {
                generalDependencyContainer.DrawingSurfaceProvider.Draw(Mupen64PlusAPI.Instance.FrameBuffer, Mupen64PlusAPI.Instance.BufferWidth, Mupen64PlusAPI.Instance.BufferHeight);
            };

            generalDependencyContainer.UIThreadDispatcherProvider.Execute(delegate
            {
                IsRunning = true;
            });

            Mupen64PlusAPI.Instance.Launch(mupen64PlusLaunchParameters);

            Mupen64PlusAPI.Instance.Dispose();

            generalDependencyContainer.UIThreadDispatcherProvider.Execute(delegate
            {
                IsRunning = false;
            });


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
