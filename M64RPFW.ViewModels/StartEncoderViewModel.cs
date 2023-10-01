using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.ViewModels;

public partial class StartEncoderViewModel  : ObservableObject
{
    [ObservableProperty] private string _path = "";
    
    public event Action<StartEncoderDialogResult?> OnCloseRequested = _ => { };
    
    public FilePickerOption[] PickerOptions => new FilePickerOption[]
    {
        new("MP4 (.mp4)", new[] {"*.mp4"}),
        new("WebM (.webm)", new[] {"*.webm"}),
    };
    
    [RelayCommand]
    private void ReturnSuccess()
    {
        OnCloseRequested(new StartEncoderDialogResult(Path));
    }

    [RelayCommand]
    private void ReturnFailure()
    {
        OnCloseRequested(null);
    }
}