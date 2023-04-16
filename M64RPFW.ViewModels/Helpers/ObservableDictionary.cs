using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace M64RPFW.ViewModels.Helpers;

/// <summary>
/// Dictionary that can notify changes.
/// The implementation is a near-complete ripoff of AvaloniaDictionary, but it's
/// implemented independently for the sake of modularity.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyPropertyChanged,
    INotifyCollectionChanged where TKey : notnull
{
    private Dictionary<TKey, TValue> _data;

    public ObservableDictionary()
    {
        _data = new Dictionary<TKey, TValue>();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        var items = _data.ToArray();
        _data.Clear();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset, items));
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>) _data).Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>) _data).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public int Count => _data.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
        ((ICollection<KeyValuePair<TKey, TValue>>) _data).IsReadOnly;

    public void Add(TKey key, TValue value)
    {
        _data.Add(key, value);
        NotifyAdd(key, value);
    }

    public bool ContainsKey(TKey key) => _data.ContainsKey(key);

    public bool Remove(TKey key)
    {
        bool found = _data.TryGetValue(key, out var prevValue);
        if (found)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{ObservableHelpers.IndexerName}"));
            _data.Remove(key);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, prevValue!)));
        }

        return found;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _data.TryGetValue(key, out value);

    public TValue this[TKey key]
    {
        get => _data[key];
        set
        {
            bool hadValue = _data.TryGetValue(key, out var prevValue);
            _data[key] = value;
            if (hadValue)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{ObservableHelpers.IndexerName}[{key}]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, prevValue!)));
            }
            else
            {
                NotifyAdd(key, value);
            }
        }
    }

    private void NotifyAdd(TKey key, TValue value)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{ObservableHelpers.IndexerName}[{key}]"));
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            new KeyValuePair<TKey, TValue>(key, value),
            -1));
    }

    public ICollection<TKey> Keys => _data.Keys;
    public ICollection<TValue> Values => _data.Values;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
}