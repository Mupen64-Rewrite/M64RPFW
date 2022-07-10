namespace M64RPFW.Models.Emulation.Provider
{
    public static class GameInfoProvider
    {
        public static string[] ValidROMFileExtensions { get; private set; } = new string[]
        {
            "z64",
            "n64",
        };

        public const int SAVESTATE_SLOT_MIN = 0;
        public const int SAVESTATE_SLOT_MAX = 10;
    }
}
