using CleanVM.Core.Interfaces;
using System.Text;

namespace CleanVM.Core.Services;

public class ISODetector : IISODetector
{
    public async Task<ISOInfo?> DetectAsync(string isoPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(isoPath))
            return null;

        var fileInfo = new FileInfo(isoPath);
        
        // Read ISO volume label from ISO9660 primary volume descriptor
        var volumeLabel = await ReadVolumeLabel(isoPath, cancellationToken);
        var bootable = await CheckBootable(isoPath, cancellationToken);
        
        // Detect OS from volume label, then fallback to filename
        var os = DetectOSFromVolumeLabel(volumeLabel) ?? DetectOSFromFilename(Path.GetFileName(isoPath));
        
        return new ISOInfo(
            isoPath,
            os,
            fileInfo.Length,
            bootable
        );
    }

    private async Task<string?> ReadVolumeLabel(string isoPath, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = File.OpenRead(isoPath);
            
            // ISO9660 Primary Volume Descriptor is at sector 16 (offset 0x8000)
            stream.Seek(0x8000, SeekOrigin.Begin);
            
            var buffer = new byte[2048]; // One sector
            await stream.ReadAsync(buffer, cancellationToken);
            
            // Volume ID is at offset 40 (0x28) and is 32 bytes
            // Check if this is a valid PVD (type code 1)
            if (buffer[0] != 1)
                return null;
            
            // Check identifier "CD001"
            var identifier = Encoding.ASCII.GetString(buffer, 1, 5);
            if (identifier != "CD001")
                return null;
            
            // Read volume ID
            var volumeId = Encoding.ASCII.GetString(buffer, 40, 32).Trim();
            return string.IsNullOrWhiteSpace(volumeId) ? null : volumeId;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> CheckBootable(string isoPath, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = File.OpenRead(isoPath);
            
            // Check for El Torito boot record at sector 17 (offset 0x8800)
            stream.Seek(0x8800, SeekOrigin.Begin);
            
            var buffer = new byte[2048];
            await stream.ReadAsync(buffer, cancellationToken);
            
            // Check for El Torito signature
            var signature = Encoding.ASCII.GetString(buffer, 7, 23);
            return signature.Contains("EL TORITO");
        }
        catch
        {
            return false;
        }
    }

    private Interfaces.OperatingSystem? DetectOSFromVolumeLabel(string? volumeLabel)
    {
        if (string.IsNullOrWhiteSpace(volumeLabel))
            return null;
        
        var lower = volumeLabel.ToLowerInvariant();
        
        // Windows detection - including MS volume label patterns
        if (lower.Contains("win") || lower.Contains("grmfrer") || lower.Contains("grmculfrer") || 
            lower.Contains("sss_") || lower.Contains("eval") || lower.Contains("_fre"))
        {
            if (lower.Contains("11") || lower.Contains("srvr_"))
                return new Interfaces.OperatingSystem("Windows", "11/Server", OSType.Windows);
            if (lower.Contains("10"))
                return new Interfaces.OperatingSystem("Windows", "10", OSType.Windows);
            return new Interfaces.OperatingSystem("Windows", "Server/Unknown", OSType.Windows);
        }
        
        // Ubuntu detection
        if (lower.Contains("ubuntu"))
        {
            var version = ExtractVersion(lower);
            return new Interfaces.OperatingSystem("Ubuntu", version, OSType.Linux);
        }
        
        // Debian
        if (lower.Contains("debian"))
        {
            var version = ExtractVersion(lower);
            return new Interfaces.OperatingSystem("Debian", version, OSType.Linux);
        }
        
        // Fedora
        if (lower.Contains("fedora"))
        {
            var version = ExtractVersion(lower);
            return new Interfaces.OperatingSystem("Fedora", version, OSType.Linux);
        }
        
        // CentOS/RHEL
        if (lower.Contains("centos") || lower.Contains("rhel"))
        {
            var version = ExtractVersion(lower);
            var name = lower.Contains("rhel") ? "Red Hat Enterprise Linux" : "CentOS";
            return new Interfaces.OperatingSystem(name, version, OSType.Linux);
        }
        
        return null;
    }

    public VMConfiguration GetRecommendedConfiguration(ISOInfo isoInfo)
    {
        return isoInfo.OS.Type switch
        {
            OSType.Windows => GetWindowsConfiguration(isoInfo),
            OSType.Linux => new VMConfiguration(
                Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
                CpuCores: 2,
                MemoryMB: 2048,
                DiskSizeGB: 20,
                IsoPath: isoInfo.Path,
                NetworkMode: NetworkMode.NAT,
                DetectedOS: isoInfo.OS
            ),
            OSType.MacOS => GetMacConfiguration(isoInfo),
            _ => new VMConfiguration(
                Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
                CpuCores: 2,
                MemoryMB: 2048,
                DiskSizeGB: 20,
                IsoPath: isoInfo.Path,
                NetworkMode: NetworkMode.NAT,
                DetectedOS: isoInfo.OS
            )
        };
    }

    private VMConfiguration GetMacConfiguration(ISOInfo isoInfo)
    {
        var version = isoInfo.OS.Version.ToLowerInvariant();
        
        // Classic Mac OS 9 - very low specs
        if (version.Contains("9"))
        {
            return new VMConfiguration(
                Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
                CpuCores: 1,
                MemoryMB: 128,  // Mac OS 9 ran on 128MB
                DiskSizeGB: 2,
                IsoPath: isoInfo.Path,
                NetworkMode: NetworkMode.NAT,
                DetectedOS: isoInfo.OS
            );
        }
        
        // Early OS X (10.0-10.3) - low specs
        if (version.Contains("10.0") || version.Contains("10.1") || 
            version.Contains("10.2") || version.Contains("10.3"))
        {
            return new VMConfiguration(
                Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
                CpuCores: 1,
                MemoryMB: 512,
                DiskSizeGB: 10,
                IsoPath: isoInfo.Path,
                NetworkMode: NetworkMode.NAT,
                DetectedOS: isoInfo.OS
            );
        }
        
        // Modern macOS - higher specs
        return new VMConfiguration(
            Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
            CpuCores: 2,
            MemoryMB: 4096,
            DiskSizeGB: 40,
            IsoPath: isoInfo.Path,
            NetworkMode: NetworkMode.NAT,
            DetectedOS: isoInfo.OS
        );
    }

    private VMConfiguration GetWindowsConfiguration(ISOInfo isoInfo)
    {
        var version = isoInfo.OS.Version.ToLowerInvariant();
        
        // Ancient DOS and Windows 3.x - VERY low specs
        if (version.Contains("dos") || version.Contains("3.1") || version.Contains("3.11"))
        {
            return new VMConfiguration(
                Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
                CpuCores: 1,
                MemoryMB: 16,  // DOS/Win3.1 ran on 16MB!
                DiskSizeGB: 1,  // 500MB-1GB is plenty
                IsoPath: isoInfo.Path,
                NetworkMode: NetworkMode.NAT,
                DetectedOS: isoInfo.OS
            );
        }
        
        // Older Windows (95, 98, ME, 2000, XP, 2003) - low specs
        if (version.Contains("95") || version.Contains("98") || version.Contains("me") ||
            version.Contains("2000") || version.Contains("xp") || version.Contains("2003") ||
            version.Contains("nt"))
        {
            return new VMConfiguration(
                Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
                CpuCores: 1,
                MemoryMB: 512,  // Old Windows runs on very little RAM
                DiskSizeGB: 10,
                IsoPath: isoInfo.Path,
                NetworkMode: NetworkMode.NAT,
                DetectedOS: isoInfo.OS
            );
        }
        
        // Modern Windows (Vista+, Server 2008+) - higher specs
        return new VMConfiguration(
            Name: Path.GetFileNameWithoutExtension(isoInfo.Path),
            CpuCores: 2,
            MemoryMB: 4096,
            DiskSizeGB: 60,
            IsoPath: isoInfo.Path,
            NetworkMode: NetworkMode.NAT,
            DetectedOS: isoInfo.OS
        );
    }

    private Interfaces.OperatingSystem DetectOSFromFilename(string filename)
    {
        var lower = filename.ToLowerInvariant();
        
        // Check for Mac FIRST (before Windows server check)
        if (lower.Contains("macos") || lower.Contains("mac os") || lower.Contains("mac") || lower.Contains("osx"))
        {
            var version = "Unknown";
            
            // Classic Mac OS (before OS X)
            if (lower.Contains("9.2")) version = "9.2";
            else if (lower.Contains("9.1")) version = "9.1";
            else if (lower.Contains("9.0")) version = "9.0";
            else if (lower.Contains("9")) version = "9";
            
            // Early OS X
            else if (lower.Contains("10.0") || lower.Contains("cheetah")) version = "X 10.0 (Cheetah)";
            else if (lower.Contains("10.1") || lower.Contains("puma")) version = "X 10.1 (Puma)";
            else if (lower.Contains("10.2") || lower.Contains("jaguar")) version = "X 10.2 (Jaguar)";
            else if (lower.Contains("10.3") || lower.Contains("panther")) version = "X 10.3 (Panther)";
            else if (lower.Contains("10.4") || lower.Contains("tiger")) version = "X 10.4 (Tiger)";
            else if (lower.Contains("10.5") || lower.Contains("leopard") && !lower.Contains("snow")) version = "X 10.5 (Leopard)";
            else if (lower.Contains("10.6") || lower.Contains("snow leopard")) version = "X 10.6 (Snow Leopard)";
            else if (lower.Contains("10.7") || lower.Contains("lion")) version = "X 10.7 (Lion)";
            else if (lower.Contains("10.8") || lower.Contains("mountain lion")) version = "X 10.8 (Mountain Lion)";
            else if (lower.Contains("10.9") || lower.Contains("mavericks")) version = "X 10.9 (Mavericks)";
            else if (lower.Contains("10.10") || lower.Contains("yosemite")) version = "X 10.10 (Yosemite)";
            else if (lower.Contains("10.11") || lower.Contains("el capitan")) version = "X 10.11 (El Capitan)";
            
            // Modern macOS (post OS X)
            else if (lower.Contains("10.12") || lower.Contains("sierra")) version = "10.12 (Sierra)";
            else if (lower.Contains("10.13") || lower.Contains("high sierra")) version = "10.13 (High Sierra)";
            else if (lower.Contains("10.14") || lower.Contains("mojave")) version = "10.14 (Mojave)";
            else if (lower.Contains("10.15") || lower.Contains("catalina")) version = "10.15 (Catalina)";
            else if (lower.Contains("11") || lower.Contains("big sur")) version = "11 (Big Sur)";
            else if (lower.Contains("12") || lower.Contains("monterey")) version = "12 (Monterey)";
            else if (lower.Contains("13") || lower.Contains("ventura")) version = "13 (Ventura)";
            else if (lower.Contains("14") || lower.Contains("sonoma")) version = "14 (Sonoma)";
            else if (lower.Contains("15") || lower.Contains("sequoia")) version = "15 (Sequoia)";
            
            // Detect architecture from filename
            string? architecture = null;
            if (lower.Contains("ppc") || lower.Contains("powerpc"))
                architecture = "PowerPC";
            else if (lower.Contains("x86_64") || lower.Contains("intel") || 
                     (lower.Contains("10.7") || lower.Contains("10.8") || lower.Contains("10.9") || 
                      lower.Contains("10.10") || lower.Contains("10.11")))
                architecture = "x86_64 (Intel)";
            else if (lower.Contains("arm") || lower.Contains("m1") || lower.Contains("m2") || 
                     lower.Contains("ventura") || lower.Contains("sonoma") || lower.Contains("sequoia"))
                architecture = "ARM64 (Apple Silicon)";
            
            return new Interfaces.OperatingSystem("macOS", version, OSType.MacOS, architecture);
        }
        
        // Windows detection - including "server" keyword
        if (lower.Contains("windows") || lower.Contains("win") || lower.Contains("server"))
        {
            var version = "Unknown";
            
            // Modern versions
            if (lower.Contains("2025")) version = "Server 2025";
            else if (lower.Contains("2022")) version = "Server 2022";
            else if (lower.Contains("2019")) version = "Server 2019";
            else if (lower.Contains("11")) version = "11";
            else if (lower.Contains("10")) version = "10";
            
            // Older versions
            else if (lower.Contains("xp")) version = "XP";
            else if (lower.Contains("2003")) version = "Server 2003";
            else if (lower.Contains("2000")) version = "2000";
            else if (lower.Contains("98")) version = "98";
            else if (lower.Contains("95")) version = "95";
            else if (lower.Contains("me")) version = "ME";
            else if (lower.Contains("nt")) version = "NT";
            else if (lower.Contains("vista")) version = "Vista";
            else if (lower.Contains("7")) version = "7";
            else if (lower.Contains("8.1")) version = "8.1";
            else if (lower.Contains("8")) version = "8";
            
            else if (lower.Contains("server")) version = "Server";
            
            return new Interfaces.OperatingSystem("Windows", version, OSType.Windows);
        }
        
        if (lower.Contains("ubuntu"))
            return new Interfaces.OperatingSystem("Ubuntu", ExtractVersion(lower), OSType.Linux);
        
        if (lower.Contains("debian"))
            return new Interfaces.OperatingSystem("Debian", ExtractVersion(lower), OSType.Linux);
        
        if (lower.Contains("fedora"))
            return new Interfaces.OperatingSystem("Fedora", ExtractVersion(lower), OSType.Linux);
        
        if (lower.Contains("centos"))
            return new Interfaces.OperatingSystem("CentOS", ExtractVersion(lower), OSType.Linux);
        
        if (lower.Contains("arch"))
            return new Interfaces.OperatingSystem("Arch Linux", "Rolling", OSType.Linux);
        
        if (lower.Contains("macos") || lower.Contains("mac os") || lower.Contains("mac") || lower.Contains("osx"))
        {
            var version = "Unknown";
            
            // Classic Mac OS (before OS X)
            if (lower.Contains("9.2")) version = "9.2";
            else if (lower.Contains("9.1")) version = "9.1";
            else if (lower.Contains("9.0")) version = "9.0";
            else if (lower.Contains("9")) version = "9";
            
            // Early OS X
            else if (lower.Contains("10.0") || lower.Contains("cheetah")) version = "X 10.0 (Cheetah)";
            else if (lower.Contains("10.1") || lower.Contains("puma")) version = "X 10.1 (Puma)";
            else if (lower.Contains("10.2") || lower.Contains("jaguar")) version = "X 10.2 (Jaguar)";
            else if (lower.Contains("10.3") || lower.Contains("panther")) version = "X 10.3 (Panther)";
            else if (lower.Contains("10.4") || lower.Contains("tiger")) version = "X 10.4 (Tiger)";
            else if (lower.Contains("10.5") || lower.Contains("leopard") && !lower.Contains("snow")) version = "X 10.5 (Leopard)";
            else if (lower.Contains("10.6") || lower.Contains("snow leopard")) version = "X 10.6 (Snow Leopard)";
            else if (lower.Contains("10.7") || lower.Contains("lion")) version = "X 10.7 (Lion)";
            else if (lower.Contains("10.8") || lower.Contains("mountain lion")) version = "X 10.8 (Mountain Lion)";
            else if (lower.Contains("10.9") || lower.Contains("mavericks")) version = "X 10.9 (Mavericks)";
            else if (lower.Contains("10.10") || lower.Contains("yosemite")) version = "X 10.10 (Yosemite)";
            else if (lower.Contains("10.11") || lower.Contains("el capitan")) version = "X 10.11 (El Capitan)";
            
            // Modern macOS (post OS X)
            else if (lower.Contains("10.12") || lower.Contains("sierra")) version = "10.12 (Sierra)";
            else if (lower.Contains("10.13") || lower.Contains("high sierra")) version = "10.13 (High Sierra)";
            else if (lower.Contains("10.14") || lower.Contains("mojave")) version = "10.14 (Mojave)";
            else if (lower.Contains("10.15") || lower.Contains("catalina")) version = "10.15 (Catalina)";
            else if (lower.Contains("11") || lower.Contains("big sur")) version = "11 (Big Sur)";
            else if (lower.Contains("12") || lower.Contains("monterey")) version = "12 (Monterey)";
            else if (lower.Contains("13") || lower.Contains("ventura")) version = "13 (Ventura)";
            else if (lower.Contains("14") || lower.Contains("sonoma")) version = "14 (Sonoma)";
            else if (lower.Contains("15") || lower.Contains("sequoia")) version = "15 (Sequoia)";
            
            // Detect architecture from filename
            string? architecture = null;
            if (lower.Contains("ppc") || lower.Contains("powerpc"))
                architecture = "PowerPC";
            else if (lower.Contains("x86_64") || lower.Contains("intel") || 
                     (lower.Contains("10.7") || lower.Contains("10.8") || lower.Contains("10.9") || 
                      lower.Contains("10.10") || lower.Contains("10.11")))
                architecture = "x86_64 (Intel)";
            else if (lower.Contains("arm") || lower.Contains("m1") || lower.Contains("m2") || 
                     lower.Contains("ventura") || lower.Contains("sonoma") || lower.Contains("sequoia"))
                architecture = "ARM64 (Apple Silicon)";
            
            return new Interfaces.OperatingSystem("macOS", version, OSType.MacOS, architecture);
        }
        
        return new Interfaces.OperatingSystem("Unknown", "Unknown", OSType.Unknown);
    }

    private string ExtractVersion(string filename)
    {
        // Try to match version with decimal (22.04, 11.6)
        var match = System.Text.RegularExpressions.Regex.Match(filename, @"[\d]+\.[\d]+");
        if (match.Success)
            return match.Value;
        
        // Try to match single number version (38, 11, 10)
        match = System.Text.RegularExpressions.Regex.Match(filename, @"[\d]+");
        return match.Success ? match.Value : "Unknown";
    }
}
