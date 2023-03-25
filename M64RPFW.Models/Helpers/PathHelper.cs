using System.Reflection;

namespace M64RPFW.Models.Helpers;

public static class PathHelper
{
    public static bool IsValid(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// Resolves a path relative to the application directory if
    /// it is within the application directory. Otherwise, returns <paramref name="path"/>
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns><paramref name="path"/> relative to the running assembly's directory, if it is in that directory.</returns>
    /// <exception cref="NotSupportedException">If the running assembly's path cannot be found.</exception>
    public static string ResolveAppRelative(string path)
    {
        string? exePath = Assembly.GetEntryAssembly()?.Location;
        if (exePath == null)
            throw new NotSupportedException("Can't find executing assembly's path");

        exePath = Directory.GetParent(exePath)?.FullName;
        if (exePath == null)
            return Path.GetFullPath(path);

        string outPath = Path.GetRelativePath(exePath, path);
        if (outPath.StartsWith($"..{Path.DirectorySeparatorChar}"))
            return Path.GetFullPath(path);
        return outPath;
    }

    public static string DerefAppRelative(string path)
    {
        if (Path.IsPathFullyQualified(path))
            return path;
        
        string? exePath = Assembly.GetEntryAssembly()?.Location;
        if (exePath == null)
            throw new NotSupportedException("Can't find executing assembly's path");

        exePath = Directory.GetParent(exePath)?.FullName;
        if (exePath == null)
            return Path.GetFullPath(path);
        return Path.GetFullPath(Path.Join(exePath, path));
    }
}