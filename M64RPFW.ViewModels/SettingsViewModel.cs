using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Settings;
using M64RPFW.ViewModels.Extensions;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public partial class SettingsViewModel
{
    /// <summary>
    /// Indicates that a setting can only be applied on the next restart.
    /// </summary>
    private class RequiresRestartAttribute : Attribute
    {}

    private static HashSet<string> _restartProps;
    
    public SettingsViewModel()
    {
        RPFWSettings.Load();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    static SettingsViewModel()
    {
        Instance = new SettingsViewModel();

        _restartProps = new HashSet<string>();
        var props = typeof(SettingsViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            if (prop.GetCustomAttribute<RequiresRestartAttribute>() != null)
            {
                _restartProps.Add(prop.Name);
            }
        }
    }

    public static SettingsViewModel Instance { get; }
    private void SetMupenSetting<T>(IntPtr section, string key, T value,
        [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        Mupen64Plus.ConfigSet(section, key, value);
        OnPropertyChanged(callerMemberName);
    }

    private void SetRPFWSetting<T>(Action<RPFWSettings, T> setter, T value,
        [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        setter(RPFWSettings.Instance, value);
        OnPropertyChanged(callerMemberName);
    }

    public void Receive(RomLoadingMessage message)
    {
        RecentRoms.InsertUniqueAtFront(message.Value);
    }
    
    public void Receive(LuaLoadingMessage message)
    {
        RecentLuaScripts.InsertUniqueAtFront(message.Value);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != null && _restartProps.Contains(e.PropertyName))
        {
            RequiresRestart = true;
        }
    }

    // HACK: notify all properties changed on this class
    public void NotifyAllPropertiesChanged()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            OnPropertyChanged(propertyInfo.Name);
        }
    }

    [ObservableProperty] private bool _requiresRestart;
}