namespace M64RPFW.ViewModels.Helpers;

/// <summary>
///     Provides helper methods for the <see cref="Directory" /> class
/// </summary>
public static class DirectoryHelper
{
    /// <summary>
    /// Gets all files under a directory recursively
    /// </summary>
    /// <param name="path">The directory's path</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of all files</returns>
    public static IEnumerable<string> GetFilesRecursively(string path)
    {
        var queue = new Queue<string>();
        queue.Enqueue(path);
        while (queue.Count > 0)
        {
            path = queue.Dequeue();
            try
            {
                foreach (string subDir in Directory.GetDirectories(path))
                {
                    queue.Enqueue(subDir);
                }
            }
            catch
            {
                // ignored
            }
            string[]? files = null;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch
            {
                // ignored
            }
            if (files != null)
            {
                foreach (string str in files)
                {
                    yield return str;
                }
            }
        }
    }
}