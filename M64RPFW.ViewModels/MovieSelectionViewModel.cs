using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Helpers;
using MupenUtilitiesRedux.Models.Interfaces;
using MupenUtilitiesRedux.Models.Serializers;
using MupenUtilitiesRedux.ViewModels;

namespace M64RPFW.ViewModels;

public partial class MovieSelectionViewModel : ObservableObject
{
    // TODO: Make the browsing behaviour callback-based (requires creating a CommandFileBrowser)
    
    private static readonly IMovieSerializer MovieSerializer = new ReflectionMovieSerializer();
    
    [ObservableProperty] private string _path = string.Empty;
    
    private MovieViewModel? _movieViewModel = null;
    
    public MovieViewModel? MovieViewModel
    {
        get => _movieViewModel;
        private set => SetProperty(ref _movieViewModel, value);
    }
    
    partial void OnPathChanged(string value)
    {
        try
        {
            MovieViewModel = new MovieViewModel(MovieSerializer.Deserialize(File.ReadAllBytes(value)),
                System.IO.Path.GetFileNameWithoutExtension(value));
        }
        catch
        {
            // ignored
            
            // there's not really much we can do without a callback-based system
            // as we rely on INPC notifications propagated from the view, we should avoid blocking the UI thread in this path
            // however, there is no such thing as an asynchronous setter so this is the best we can do with this approach :P
        }
    }
}