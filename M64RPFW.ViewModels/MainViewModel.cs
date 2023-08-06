using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Services;

namespace M64RPFW.ViewModels;

public class MainViewModel : ObservableObject
{
    public EmulatorViewModel EmulatorViewModel { get; }
}