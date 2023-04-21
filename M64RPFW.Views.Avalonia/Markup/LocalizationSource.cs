using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace M64RPFW.Views.Avalonia.Markup;

public class LocalizationSource : INotifyPropertyChanged
{
    public static LocalizationSource Instance { get; } = new();

    private readonly ResourceManager _resourceManager = Resources.Resources.ResourceManager;
    private CultureInfo _currentCulture = null!;
    public string this[string key] => _resourceManager.GetString(key, _currentCulture) ?? $"[{key}]";

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    _currentCulture = value;
            var @event = PropertyChanged;
            // "Item" is Avalonia's special string representing the index
            @event?.Invoke(this, new PropertyChangedEventArgs("Item"));
        }
    }

    // private class ObservableSource : IObservable<string>
    // {
    //     public ObservableSource(LocalizationSource parent, string key)
    //     {
    //         _parent = parent;
    //         _observers = new HashSet<IObserver<string>>();
    //         _key = key;
    //
    //         _parent.PropertyChanged += (_, _) =>
    //         {
    //             var value = _parent[_key];
    //             foreach (var obs in _observers)
    //                 obs.OnNext(value);
    //         };
    //     }
    //
    //     private class Disposer : IDisposable
    //     {
    //         public Disposer(ObservableSource parent, IObserver<string> observer)
    //         {
    //             _parent = parent;
    //             _observer = observer;
    //             _parent._observers.Add(observer);
    //         }
    //         
    //         public void Dispose()
    //         {
    //             _parent._observers.Remove(_observer);
    //         }
    //         
    //         private ObservableSource _parent;
    //         private IObserver<string> _observer;
    //     }
    //     
    //     public IDisposable Subscribe(IObserver<string> observer)
    //     {
    //         return new Disposer(this, observer);
    //     }
    //
    //     private LocalizationSource _parent;
    //     private HashSet<IObserver<string>> _observers;
    //     private string _key;
    // }
    //
    // public IObservable<string> GetObservable(string key)
    // {
    //     return new ObservableSource(this, key);
    // }


    public event PropertyChangedEventHandler? PropertyChanged;
}