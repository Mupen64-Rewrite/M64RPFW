namespace M64RPFW.ViewModels.Helpers;

public static class FileHelpers
{
    public async static Task<byte[]> ReadSectionAsync(string path, long off, long len)
    {
        using var file = File.OpenRead(path);
        file.Seek(off, SeekOrigin.Begin);

        byte[] data = new byte[len];
        await file.ReadExactlyAsync(data);

        return data;
    }
}