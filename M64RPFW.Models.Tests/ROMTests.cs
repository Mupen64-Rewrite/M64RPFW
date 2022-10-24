using M64RPFW.Models.Emulation;

namespace M64RPFW.Models.Tests
{
    [TestClass]
    public class ROMTests
    {
        private ROM GetTestROM() => new ROM(File.ReadAllBytes("m64p_test_rom.v64".ToBundledPath()));
        

        [TestMethod]
        public void TestROMIsExtractedCorrectly()
        {
            var rom = GetTestROM();

            Assert.IsNotNull(rom, $"ROM is null");
            Assert.IsTrue(rom.IsValid, $"ROM is invalid");
            Assert.IsTrue(rom.IsBigEndian, $"ROM has wrongly identified endiandiness. Expected Big Endian, got Little Endian");
            Assert.IsTrue(rom.PrimaryCRC == 0xE54DBADD, $"ROM has mismatched primary CRC. Expected {string.Format("0x{0:X8}", 0xE54DBADD)}, got {string.Format("0x{0:X8}", rom.PrimaryCRC)}");
            Assert.IsTrue(rom.SecondaryCRC == 0x4A0007B1, $"ROM has mismatched secondary CRC. Expected {string.Format("0x{0:X8}", 0x4A0007B1)}, got {string.Format("0x{0:X8}", rom.SecondaryCRC)}");
            Assert.IsTrue(rom.CountryCode == 0, $"ROM has a mismatched country code. Expected {0}, got {rom.CountryCode}");
            Assert.IsTrue(rom.RawData.Length == 0x100000, $"ROM has a mismatched raw data length. Expected {0x100000}, got {rom.RawData.Length}");
            Assert.IsTrue(rom.InternalName == "Mupen64Plus".PadRight(20, (char)0x20), $"ROM has a mismatched internal name. Expected {"Mupen64Plus".PadRight(20, (char)0x20)}, got {rom.InternalName}");
        }
    }
}