using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.Helpers;

/// <summary>
/// Error thrown by Mupen64Plus.
/// </summary>
public class MupenException : ApplicationException
{
    public Error ErrorCode { get; }
    
    public MupenException(Error error) : base(Mupen64Plus.GetErrorMessage(error))
    {
        ErrorCode = error;
    }
    public MupenException(Error error, Exception inner) : base(Mupen64Plus.GetErrorMessage(error), inner)
    {
        ErrorCode = error;
    }
}