
using M64RPFW.Models.Emulation;

namespace M64RPFW.Models.Tests
{
    [TestClass]
    public class RomTests
    {
        private Rom GetTestRom()
        {
            return new Rom(File.ReadAllBytes("m64p_test_Rom.v64".ToBundledPath()));
        }

        [TestMethod]
        public void TestRomIsExtractedCorrectly()
        {
            Rom rom = GetTestRom();

            Assert.IsNotNull(rom, $"Rom is null");
            Assert.IsTrue(rom.IsValid, $"Rom is invalid");
            Assert.IsTrue(rom.IsBigEndian, $"Rom has wrongly identified endiandiness. Expected Big Endian, got Little Endian");
            Assert.IsTrue(rom.PrimaryCRC == 0xE54DBADD, $"Rom has mismatched primary CRC. Expected {string.Format("0x{0:X8}", 0xE54DBADD)}, got {string.Format("0x{0:X8}", rom.PrimaryCRC)}");
            Assert.IsTrue(rom.SecondaryCRC == 0x4A0007B1, $"Rom has mismatched secondary CRC. Expected {string.Format("0x{0:X8}", 0x4A0007B1)}, got {string.Format("0x{0:X8}", rom.SecondaryCRC)}");
            Assert.IsTrue(rom.CountryCode == 0, $"Rom has a mismatched country code. Expected {0}, got {rom.CountryCode}");
            Assert.IsTrue(rom.RawData.Length == 0x100000, $"Rom has a mismatched raw data length. Expected {0x100000}, got {rom.RawData.Length}");
            Assert.IsTrue(rom.InternalName == "Mupen64Plus".PadRight(20, (char)0x20), $"Rom has a mismatched internal name. Expected {"Mupen64Plus",-20}, got {rom.InternalName}");
        }
    }
}