using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Views;

public partial class SettingsDialog : Window
{
    private Dictionary<string, object> _initialSettingsValues = new();

    public SettingsDialog()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = SettingsViewModel.Instance;
        SettingsViewModel.Instance.RequiresRestart = false;

        // duplicate and store all properties from settings vm, so we can restore them if user clicks "Cancel"
        // this is not very good for performance, since it involves not only reflection, but also a massive dictionary 
        var props = typeof(SettingsViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.SetMethod != null);
        foreach (var prop in props)
        {
            var value = prop.GetValue(SettingsViewModel.Instance);
            Debug.Assert(value != null);
            _initialSettingsValues[prop.Name] = value;
        }
    }

    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext!;

    private void OnApplyClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.SaveCommand.Execute(null);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        foreach (var pair in _initialSettingsValues)
        {
            var property = typeof(SettingsViewModel).GetProperty(pair.Key, BindingFlags.Public | BindingFlags.Instance);
            Debug.Assert(property != null);
            property.SetValue(SettingsViewModel.Instance, pair.Value);
        }
        ViewModel.NotifyAllPropertiesChanged();

        ViewModel.RequiresRestart = false;
        Close();
    }

    private void OnOKClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.SaveCommand.Execute(null);
        ViewModel.RequiresRestart = false;
        Close();
    }
}