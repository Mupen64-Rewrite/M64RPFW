using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.ViewModels;

public partial class StartEncoderViewModel : ObservableObject
{
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private int _encodeWidth = 640;
    [ObservableProperty] private int _encodeHeight = 480;
    [ObservableProperty] private bool _useWindowSize = true;

    public event Action<StartEncoderDialogResult?> OnCloseRequested = _ => { };

    public FilePickerOption[] PickerOptions => new FilePickerOption[]
    {
        new("MP4 (.mp4)", new[]
        {
            "*.mp4"
        }),
        new("WebM (.webm)", new[]
        {
            "*.webm"
        }),
        new("MP3, audio-only (.mp3)", new[]
        {
            "*.mp3"
        }),
    };

    [RelayCommand]
    private void ReturnSuccess()
    {
        OnCloseRequested(new StartEncoderDialogResult(
            Path, UseWindowSize ? null : new WindowSize(EncodeWidth, EncodeHeight)));
    }

    [RelayCommand]
    private void ReturnFailure()
    {
        OnCloseRequested(null);
    }
}