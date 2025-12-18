# ISO Detection

CleanVM's ISO detector automatically identifies operating systems from ISO files without requiring the user to specify OS type, version, or architecture.

## How It Works

### 1. Volume Label Detection
Reads the ISO9660 volume label from the ISO file metadata.

### 2. Boot Record Analysis
Parses El Torito boot records to determine if the ISO is bootable.

### 3. Filename Pattern Matching
Falls back to intelligent filename parsing when volume labels are generic.

### 4. Architecture Detection
Determines CPU architecture (PowerPC, Intel x86_64, ARM64) from filename patterns and file contents.

## Supported Operating Systems

### Windows
- **Legacy**: DOS, Windows 3.1, 95, 98, ME
- **NT Family**: 2000, XP, Vista, 7, 8, 8.1, 10, 11
- **Server**: 2003, 2008, 2012, 2016, 2019, 2022, 2025

### Linux
- **Ubuntu**: All versions
- **Fedora**: All versions
- **Arch Linux**: Rolling release
- **Debian**: All versions
- **RHEL/CentOS**: All versions

### macOS
- **Classic**: Mac OS 9.x
- **OS X**: 10.0 (Cheetah) through 10.11 (El Capitan)
- **macOS**: 10.12 (Sierra) through 15 (Sequoia)

## Era-Appropriate Configuration

CleanVM recommends specs based on the OS era:

| Operating System | RAM | CPU | Disk | Notes |
|-----------------|-----|-----|------|-------|
| DOS / Win 3.1 | 16MB | 1 | 1GB | Accurate for 1990s |
| Win 95-XP | 512MB | 1 | 10GB | Legacy Windows |
| Modern Windows/Linux | 4GB | 2 | 60GB | Current systems |
| Mac OS 9 | 128MB | 1 | 2GB | PowerPC era |
| OS X 10.3-10.7 | 512MB | 1 | 10GB | Early Intel Macs |
| Modern macOS | 4GB | 2 | 40GB | Apple Silicon ready |

## Usage Example

```csharp
using CleanVM.Core.Services;

var detector = new ISODetector();
var isoInfo = await detector.DetectAsync("/path/to/ubuntu-24.04.iso");

if (isoInfo != null)
{
    Console.WriteLine($"OS: {isoInfo.OS.Name} {isoInfo.OS.Version}");
    Console.WriteLine($"Type: {isoInfo.OS.Type}");
    Console.WriteLine($"Architecture: {isoInfo.OS.Architecture ?? "Unknown"}");
    Console.WriteLine($"Bootable: {isoInfo.IsBootable}");
    
    var config = detector.GetRecommendedConfiguration(isoInfo);
    Console.WriteLine($"Recommended specs:");
    Console.WriteLine($"  CPUs: {config.CpuCores}");
    Console.WriteLine($"  RAM: {config.MemoryMB}MB");
    Console.WriteLine($"  Disk: {config.DiskSizeGB}GB");
}
```

## Architecture Detection

CleanVM can detect CPU architecture from:

1. **Filename patterns**:
   - `ppc` or `powerpc` → PowerPC
   - `x86_64` or `intel` → Intel x86_64
   - `arm` or `m1` or `m2` → ARM64 (Apple Silicon)

2. **Version inference**:
   - Mac OS 9 → PowerPC
   - OS X 10.7-10.11 → Intel
   - macOS 12+ → Universal (Intel + ARM)

3. **File inspection** (future):
   - Read executable headers
   - Check boot loaders
   - Analyze kernel binaries

## Testing

CleanVM's ISO detector has been validated with:

- 18 real Windows ISOs (DOS through Server 2025)
- 11 Linux distributions (multiple variants)
- 4 macOS versions (OS 9 through Ventura)
- **Total**: 84GB of real-world test data

## Advanced Features

### Deep Inspection
The `ISODeepInspector` class can read files inside ISOs without mounting:

```csharp
var deepInspector = new ISODeepInspector();

// List all files in ISO
var files = await deepInspector.ListFilesAsync(isoPath);

// Read specific file
var readme = await deepInspector.ReadFileAsync(isoPath, "/README.TXT");

// Detect Windows version from install.wim
var version = await deepInspector.DetectWindowsVersionAsync(isoPath);
```

### ISO Library Management
Cache and search large ISO collections:

```csharp
var library = new ISOLibraryManager();

// Scan directory
await library.ScanDirectoryAsync("/path/to/isos");

// Search
var ubuntu = library.Search("ubuntu");
var servers = library.FilterByType(OSType.Windows)
                     .Where(iso => iso.OS.Version.Contains("Server"));
```

## Limitations

1. **Volume labels must be informative**: Generic labels like "CDROM" can't be detected
2. **UDF ISOs require 7z**: Modern Windows ISOs use UDF, need 7z for deep inspection
3. **Mac DMG files**: Not yet supported (coming soon)

## Future Enhancements

- [ ] DMG file support for modern macOS
- [ ] Binary inspection for architecture detection
- [ ] Install.wim parsing for exact Windows editions
- [ ] Automatic ISO download from official sources
