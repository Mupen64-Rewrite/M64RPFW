using M64RPFW.Models.Emulation;

namespace M64RPFW.Models.Tests;

[TestClass]
public class RomTests
{
    private static Rom GetTestRom()
    {
        return new Rom(File.ReadAllBytes("m64p_test_rom.v64".ToBundledPath()));
    }

    [TestMethod]
    public void TestRomIsExtractedCorrectly()
    {
        var rom = GetTestRom();

        Assert.IsNotNull(rom, "Rom is null");
        Assert.IsTrue(rom.IsValid, "Rom is invalid");
        Assert.IsTrue(rom.IsBigEndian,
            "Rom has wrongly identified endiandiness. Expected Big Endian, got Little Endian");
        Assert.IsTrue(rom.PrimaryCrc == 0xE54DBADD,
            $"Rom has mismatched primary CRC. Expected 0x{0xE54DBADD:X8}, got 0x{rom.PrimaryCrc:X8}");
        Assert.IsTrue(rom.SecondaryCrc == 0x4A0007B1,
            $"Rom has mismatched secondary CRC. Expected 0x{0x4A0007B1:X8}, got 0x{rom.SecondaryCrc:X8}");
        Assert.IsTrue(rom.CountryCode == 0, $"Rom has a mismatched country code. Expected {0}, got {rom.CountryCode}");
        Assert.IsTrue(rom.RawData.Length == 0x100000,
            $"Rom has a mismatched raw data length. Expected {0x100000}, got {rom.RawData.Length}");
        Assert.IsTrue(rom.InternalName == "Mupen64Plus".PadRight(20, (char)0x20),
            $"Rom has a mismatched internal name. Expected {"Mupen64Plus",-20}, got {rom.InternalName}");
    }
}