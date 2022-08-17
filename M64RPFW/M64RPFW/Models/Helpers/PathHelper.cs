using System;
using System.IO;

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
}