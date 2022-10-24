namespace M64RPFW.Models.Emulation.Exceptions
{
    internal class UnresolvableConfigEntryTypeException : Exception
    {
        public UnresolvableConfigEntryTypeException(string? message) : base(message)
        {
        }
    }
}
