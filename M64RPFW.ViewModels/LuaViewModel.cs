﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Scripting;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.ViewModels;

public partial class LuaViewModel : ObservableObject
{
    private readonly IFrontendScriptingService _frontendScriptingService;
    private LuaEnvironment? _luaEnvironment;

    [ObservableProperty] private string _path = "C:\\Users\\Alex\\Desktop\\test.lua";

    public bool IsRunning { get; private set; }

    public FilePickerOption[] PickerOptions => new FilePickerOption[]
    {
        new("Lua script (.lua)", new[] { "*.lua" })
    };

    public LuaViewModel(IFrontendScriptingService frontendScriptingService)
    {
        _frontendScriptingService = frontendScriptingService;
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void Stop()
    {
        _luaEnvironment?.Dispose();
    }

    [RelayCommand]
    private void Run()
    {
        if (IsRunning) StopCommand.Execute(null);

        _luaEnvironment = new LuaEnvironment(_frontendScriptingService, Path);
        _luaEnvironment.StateChanged += LuaEnvironmentStateChanged;
        _luaEnvironment.Run();
    }

    private void LuaEnvironmentStateChanged(bool obj)
    {
        IsRunning = obj;
        OnPropertyChanged(nameof(IsRunning));
        StopCommand.NotifyCanExecuteChanged();
    }
}