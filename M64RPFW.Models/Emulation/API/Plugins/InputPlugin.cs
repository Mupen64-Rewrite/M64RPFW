namespace M64RPFW.Models.Emulation.API.Plugins;

internal class InputPlugin : Plugin
{
    public InputPlugin(Mupen64PlusTypes.EmulatorPluginType type, IntPtr handle) : base(type, handle)
    {
    }
}