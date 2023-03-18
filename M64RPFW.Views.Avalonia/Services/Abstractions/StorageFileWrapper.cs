using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Services.Abstractions;

/// <summary>
/// An implementation of <see cref="IFile"/> using <see cref="IStorageFile"/>
/// </summary>
public class StorageFileWrapper : IFile
{
    public StorageFileWrapper(IStorageFile file)
    {
        _file = file;
    }

    private IStorageFile _file;

    /// <inheritdoc />
    public string DisplayName => _file.Name;
    /// <inheritdoc />
    public string Path => _file.Path.LocalPath;
    /// <inheritdoc />
    public bool IsReadOnly => !_file.CanOpenWrite;
    /// <inheritdoc />
    public async Task<(ulong Size, DateTimeOffset EditTime)> GetPropertiesAsync()
    {
        var props = await _file.GetBasicPropertiesAsync();
        if (props is not { Size: { }, DateModified: { } })
            throw new InvalidOperationException("Platform does not support getting file properties");
        return (props.Size!.Value, props.DateModified!.Value);
    }

    /// <inheritdoc />
    public Task<Stream> OpenStreamForReadAsync()
    {
        return _file.OpenReadAsync();
    }

    /// <inheritdoc />
    public Task<Stream> OpenStreamForWriteAsync()
    {
        return _file.OpenWriteAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync()
    {
        var path = _file.Path.LocalPath;
        await Task.Run(() => System.IO.File.Delete(path));
    }

    /// <inheritdoc />
    public void RequestFutureAccessPermission(string metadata)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void RemoveFutureAccessPermission()
    {
        throw new NotImplementedException();
    }
}