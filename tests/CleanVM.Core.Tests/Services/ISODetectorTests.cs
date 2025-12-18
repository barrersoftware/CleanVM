using CleanVM.Core.Services;
using CleanVM.Core.Interfaces;

namespace CleanVM.Core.Tests.Services;

public class ISODetectorTests
{
    [Fact]
    public void GetRecommendedConfiguration_Windows_ShouldReturnHigherSpecs()
    {
        var detector = new ISODetector();
        var isoInfo = new ISOInfo(
            "/path/to/windows.iso",
            new Interfaces.OperatingSystem("Windows", "11", OSType.Windows),
            5L * 1024 * 1024 * 1024,
            true
        );

        var config = detector.GetRecommendedConfiguration(isoInfo);

        Assert.Equal(2, config.CpuCores);
        Assert.Equal(4096, config.MemoryMB); // Windows needs more RAM
        Assert.Equal(60, config.DiskSizeGB); // Windows needs more disk
    }

    [Fact]
    public void GetRecommendedConfiguration_Linux_ShouldReturnModerateSpecs()
    {
        var detector = new ISODetector();
        var isoInfo = new ISOInfo(
            "/path/to/ubuntu.iso",
            new Interfaces.OperatingSystem("Ubuntu", "22.04", OSType.Linux),
            3L * 1024 * 1024 * 1024,
            true
        );

        var config = detector.GetRecommendedConfiguration(isoInfo);

        Assert.Equal(2, config.CpuCores);
        Assert.Equal(2048, config.MemoryMB); // Linux more efficient
        Assert.Equal(20, config.DiskSizeGB);
    }

    [Theory]
    [InlineData("ubuntu-22.04-desktop-amd64.iso", "Ubuntu", "22.04", OSType.Linux)]
    [InlineData("debian-11.6.0-amd64-DVD-1.iso", "Debian", "11.6", OSType.Linux)]
    [InlineData("Win11_22H2_English_x64.iso", "Windows", "11", OSType.Windows)]
    [InlineData("Windows-Server-2022.iso", "Windows", "Server", OSType.Windows)]
    [InlineData("fedora-38-x86_64.iso", "Fedora", "38", OSType.Linux)]
    public void DetectOSFromFilename_ShouldDetectCorrectly(string filename, string expectedName, string expectedVersion, OSType expectedType)
    {
        var detector = new ISODetector();
        
        // Use reflection to call private method (test helper)
        var method = typeof(ISODetector).GetMethod("DetectOSFromFilename", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = method?.Invoke(detector, new object[] { filename }) as Interfaces.OperatingSystem;

        Assert.NotNull(result);
        Assert.Equal(expectedName, result.Name);
        Assert.Contains(expectedVersion, result.Version);
        Assert.Equal(expectedType, result.Type);
    }
}
