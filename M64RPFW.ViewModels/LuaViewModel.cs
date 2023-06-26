using CommunityToolkit.Mvvm.ComponentModel;
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

    public FilePickerOption[] PickerOptions => new FilePickerOption[]
    {
        new("Lua script (.lua)", new[] { "*.lua" })
    };
    
    public LuaViewModel(IFrontendScriptingService frontendScriptingService)
    {
        _frontendScriptingService = frontendScriptingService;
    }

    [RelayCommand]
    private void Stop()
    {
        _luaEnvironment?.Dispose();
    }

    [RelayCommand]
    private void Run()
    {
        StopCommand.Execute(null);
        _luaEnvironment = new(_frontendScriptingService);
        _luaEnvironment.Run(Path);
    }
}