using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Types;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.ViewModels;

public partial class OpenMovieViewModel : ObservableObject
{
    [ObservableProperty] private string _path;
    [ObservableProperty] private string _authors;
    [ObservableProperty] private string _description;
    [ObservableProperty] private Mupen64PlusTypes.VCRStartType _startType;
    
    public OpenMovieViewModel() {}

    public event Action<OpenMovieDialogResult?> OnCloseRequested = _ => {};
}