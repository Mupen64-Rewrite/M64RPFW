namespace M64RPFW.Models.Tests;

internal static class TestIOProvider
{
    internal static string ToBundledPath(this string fileName)
    {
        return Path.Combine("Bundled", fileName);
    }
}