using System.Collections.ObjectModel;

namespace M64RPFW.ViewModels.Extensions;

/// <summary>
///     Provides extension methods for collection types
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Inserts a uniquely referenced item at the head of the list
    /// </summary>
    /// <param name="list">The list to be modified</param>
    /// <param name="item">The item to be inserted</param>
    /// <typeparam name="T">The list's type</typeparam>
    public static void InsertUniqueAtFront<T>(this Collection<T> list, T item)
    {
        list.Remove(item);
        list.Insert(0, item);
    }


    /// <summary>
    /// Adds a collection of items to an <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <param name="observableCollection">The <see cref="ObservableCollection{T}"/> to add the items to</param>
    /// <param name="items">The items to be added</param>
    /// <typeparam name="T">The <see cref="ObservableCollection{T}"/> and <see cref="ICollection{T}"/>'s type</typeparam>
    public static void AddRange<T>(this ObservableCollection<T> observableCollection, ICollection<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        foreach (var item in items)
        {
            observableCollection.Add(item);
        }
    }
    
}