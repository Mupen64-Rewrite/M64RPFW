using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Scripting;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public partial class LuaViewModel : ObservableObject
{
    private readonly ILuaWindowService _luaWindowService;
    private readonly ILuaInterfaceService _windowSizingService;
    private readonly IFilePickerService _filePickerService;

    private LuaEnvironment? _luaEnvironment;

    [ObservableProperty] private string _path = string.Empty;

    public bool IsRunning { get; private set; }

    public FilePickerOption[] PickerOptions => new FilePickerOption[]
    {
        new("Lua script (.lua)", new[]
        {
            "*.lua"
        })
    };

    public LuaViewModel(ILuaWindowService luaWindowService, ILuaInterfaceService windowSizingService, IFilePickerService filePickerService)
    {
        _luaWindowService = luaWindowService;
        _windowSizingService = windowSizingService;
        _filePickerService = filePickerService;
        
        // restore most recent path, like old mupen
        if (SettingsViewModel.Instance.RecentLuaScripts.Count > 0)
        {
            Path = SettingsViewModel.Instance.RecentLuaScripts[0];
        }
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void Stop()
    {
        _luaEnvironment?.Dispose();
    }

    [RelayCommand]
    private void Run()
    {
        if (IsRunning)
            Stop();

        _luaEnvironment = new LuaEnvironment(Path, _luaWindowService, _windowSizingService, _filePickerService);
        _luaEnvironment.StateChanged += LuaEnvironmentStateChanged;
        if (_luaEnvironment.Run())
        {
            WeakReferenceMessenger.Default.Send(new LuaLoadingMessage(Path));
        }
    }

    private void LuaEnvironmentStateChanged(bool obj)
    {
        IsRunning = obj;
        OnPropertyChanged(nameof(IsRunning));
        StopCommand.NotifyCanExecuteChanged();
    }
}