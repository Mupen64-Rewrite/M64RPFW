using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace M64RPFW.Wpf.Interfaces;

internal sealed class PinnedString : CriticalFinalizerObject
{
    public PinnedString(string value)
    {
        pin = GCHandle.Alloc(value.ToCharArray(), GCHandleType.Pinned);
    }

    public static unsafe implicit operator char*(PinnedString str)
    {
        return (char*) str.pin.AddrOfPinnedObject();
    }

    public static unsafe implicit operator PCWSTR(PinnedString str)
    {
        return (char*) str.pin.AddrOfPinnedObject();
    }

    ~PinnedString()
    {
        pin.Free();
    }
    
    private GCHandle pin;
}