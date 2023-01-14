using System.Runtime.InteropServices;
using static M64RPFW.Models.Emulation.API.Mupen64PlusDelegates;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API.Plugins;

internal class VideoPlugin : Plugin
{
    public VideoPlugin(EmulatorPluginType type, IntPtr handle) : base(type, handle)
    {
        ReadScreen2 = GetDelegateFromLibrary<ReadScreen2Delegate>(handle);
        ReadScreen2Res = GetDelegateFromLibrary<ReadScreen2ResDelegate>(handle);

        var funcPtr = GetProcAddress(handle, "GetScreenTextureID");
        if (funcPtr != IntPtr.Zero)
            GetScreenTextureId =
                (GetScreenTextureIdDelegate)Marshal.GetDelegateForFunctionPointer(funcPtr,
                    typeof(GetScreenTextureIdDelegate));
    }

    internal ReadScreen2Delegate ReadScreen2 { get; }
    internal ReadScreen2ResDelegate ReadScreen2Res { get; }
    internal GetScreenTextureIdDelegate GetScreenTextureId { get; }
}