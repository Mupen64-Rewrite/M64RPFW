using M64RPFW.Models.Emulation;

namespace M64RPFW.Models.Settings;

/// <summary>
/// Static class that holds settings handles for Mupen64Plus.
/// </summary>
public static class MupenSettings
{
    static MupenSettings()
    {
        Core = Mupen64Plus.ConfigOpenSection("Core");
        VideoGeneral = Mupen64Plus.ConfigOpenSection("Video-General");
    }

    /// <summary>
    /// The "Core" section of the Mupen64Plus settings. Contains
    /// settings relating to the emulator itself.
    /// </summary>
    public static IntPtr Core { get; }

    /// <summary>
    /// The "Video-General" section of Mupen64Plus settings. Contains
    /// settings for basic video output.
    /// </summary>
    public static IntPtr VideoGeneral { get; }

    /// <summary>
    /// Does the exact same thing as Mupen64Plus's <c>ConfigGetUserConfigPath().</c>
    /// Resolves to %APPDATA%\Mupen64Plus on Windows, and ~/.config/mupen64plus on
    /// most Unix-based systems.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown on Unix-based systems if neither $HOME or $XDG_CONFIG_HOME is set.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the system is not supported.
    /// </exception>
    internal static string GetUserConfigPath()
    {
        if (OperatingSystem.IsWindows())
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // ReSharper disable once InconsistentNaming
            var m64pDir = Directory.CreateDirectory(Path.Join(appDataPath, "Mupen64Plus"));

            return m64pDir.FullName;
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            DirectoryInfo? dir = null;
            {
                string? envConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (envConfigHome != null && Directory.Exists(envConfigHome))
                    dir = Directory.CreateDirectory(Path.Join(envConfigHome, "mupen64plus"));
            }
            if (dir == null)
            {
                string? envHome = Environment.GetEnvironmentVariable("HOME");
                if (envHome != null && Directory.Exists(envHome))
                    dir = Directory.CreateDirectory(Path.Join(envHome, ".config/mupen64plus"));
            }

            if (dir == null)
                throw new FileNotFoundException("No locatable config path was found.");

            return dir.FullName;
        }

        throw new NotSupportedException("Are you seriously trying to run this on a phone?");
    }
}