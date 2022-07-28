using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFWAvalonia.Models;
using System.Configuration;

namespace M64RPFWAvalonia.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {

        [RelayCommand]
        private void SetSettingsProperty(string virtualArguments)
        {
            (string Key, string Value) args = SettingsVirtualArgumentHelper.ParseVirtualArgument(virtualArguments);
            ConfigurationManager.AppSettings[args.Key] = args.Value;
        }


    }
}
