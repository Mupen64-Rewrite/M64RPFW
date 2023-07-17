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
    
    
}