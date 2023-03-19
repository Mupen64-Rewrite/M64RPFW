namespace M64RPFW.Models.Helpers;

public static class ReflectionExtensions
{
    /// <summary>
    /// Returns true if the specified type is an integer type.
    /// </summary>
    /// <param name="t">the type to check</param>
    /// <returns>true if <paramref name="t"/> is an integer type</returns>
    public static bool IsIntegerType(this Type t)
    {
        switch (Type.GetTypeCode(t))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }
}