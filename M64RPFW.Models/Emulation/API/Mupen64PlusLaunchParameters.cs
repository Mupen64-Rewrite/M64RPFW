using M64RPFW.Services.Abstractions;

namespace M64RPFW.Models.Emulation.API;

public class Mupen64PlusLaunchParameters
{
    public readonly IFile AudioPlugin;
    public readonly Mupen64PlusConfig Config;
    public readonly IFile CoreLibrary;
    public readonly int InitialSlot;
    public readonly IFile InputPlugin;
    public readonly byte[] Rom;
    public readonly IFile RspPlugin;
    public readonly IFile VideoPlugin;

    public Mupen64PlusLaunchParameters(byte[] rom, Mupen64PlusConfig config, int initialSlot, IFile coreLibrary,
        IFile videoPlugin, IFile audioPlugin, IFile inputPlugin, IFile rspPlugin)
    {
        Rom = rom;
        Config = config;
        InitialSlot = initialSlot;
        CoreLibrary = coreLibrary;
        VideoPlugin = videoPlugin;
        AudioPlugin = audioPlugin;
        InputPlugin = inputPlugin;
        RspPlugin = rspPlugin;
    }
}