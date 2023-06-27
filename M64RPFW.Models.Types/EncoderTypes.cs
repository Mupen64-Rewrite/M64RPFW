using System.Runtime.InteropServices;

namespace M64RPFW.Models.Types;

public partial class Mupen64PlusTypes
{
    public enum EncoderFormat : int
    {
        Infer = -1,
        Mp4 = 0,
        Webm = 1,
        Mov = 2
    }

    
}