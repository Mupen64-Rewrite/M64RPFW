using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public unsafe partial class Mupen64Plus
{
    public static bool Encoder_IsActive()
    {
        return _encoderIsActive();
    }

    public static void Encoder_Start(string path, Mupen64PlusTypes.EncoderFormat fmt)
    {
        byte[] pathData = Encoding.ASCII.GetBytes(path + char.MinValue);
        fixed (byte* pPathData = pathData)
        {
            _encoderStart(pPathData, Mupen64PlusTypes.EncoderFormat.Infer);
        }
    }
}