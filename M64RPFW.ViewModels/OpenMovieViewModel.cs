using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Movies;
using M64RPFW.Services.Abstractions;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.ViewModels;

public partial class OpenMovieViewModel : ObservableObject
{
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _authors = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private VCRStartType _startType = VCRStartType.Snapshot;
    [ObservableProperty] private bool _isEditable = true;

    public event Action<OpenMovieDialogResult?> OnCloseRequested = _ => { };

    public VCRStartType[] StartTypes => Enum.GetValues<VCRStartType>();

    public FilePickerOption[] PickerOptions => new FilePickerOption[]
    {
        new("Mupen64 movie (.m64)", new[] {"*.m64"})
    };

    partial void OnPathChanged(string value)
    {
        if (IsEditable || !File.Exists(value))
            return;
        
        // Load existing info (this might be slow depending on disk speed, but...)
        MovieHeader mh = new();
        mh.Load(value);

        Authors = mh.Authors;
        Description = mh.Description;
        StartType = (VCRStartType) mh.StartType;
    }

    [RelayCommand]
    private void ReturnSuccess()
    {
        OnCloseRequested(new OpenMovieDialogResult(Path, Authors, Description, StartType));
    }

    [RelayCommand]
    private void ReturnFailure()
    {
        OnCloseRequested(null);
    }
}