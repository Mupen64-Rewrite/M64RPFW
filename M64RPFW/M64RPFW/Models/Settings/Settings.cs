using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Emulation.Core;
using Tomlyn;

namespace M64RPFW.Models.Settings;

public static class Settings
{
    static Settings()
    {
        string path = Path.Join(GetUserConfigPath(), "RPFW.toml");
        Console.WriteLine($"Config path: {path}");
        if (!File.Exists(path))
        {
            // Simply create an empty file
            File.Create(path).Dispose();
        }

        RPFW = Toml.ToModel<RPFWSettings>(File.ReadAllText(path, Encoding.UTF8));

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            File.WriteAllText(path, Toml.FromModel(RPFW), Encoding.UTF8);
        };

        Core = new CoreSettings();
        VideoGeneral = new VideoGeneralSettings();
    }
    
    /// <summary>
    /// Does absolutely nothing except ensure
    /// that the static constructor is called.
    /// </summary>
    public static void Init() {}

    public static void EnsureSaved()
    {
        string path = Path.Join(GetUserConfigPath(), "RPFW.toml");
        File.WriteAllText(path, Toml.FromModel(RPFW), Encoding.UTF8);
        
        Mupen64Plus.ConfigSaveFile();
    }
    
    public static RPFWSettings RPFW { get; }
    
    public static CoreSettings Core { get; }
    public static VideoGeneralSettings VideoGeneral { get; }
    
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
    /// Thrown if the system is neither Windows nor Unix.
    /// </exception>
    private static string GetUserConfigPath()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // ReSharper disable once InconsistentNaming
            var m64pDir = Directory.CreateDirectory(Path.Join(appDataPath, "Mupen64Plus"));

            return m64pDir.FullName;
        }
        if (Environment.OSVersion.Platform == PlatformID.Unix)
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
        throw new NotSupportedException("You're daily-driving a very esoteric OS.");
    }
}