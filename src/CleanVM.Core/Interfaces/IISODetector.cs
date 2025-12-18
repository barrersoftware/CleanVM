namespace CleanVM.Core.Interfaces;

public interface IISODetector
{
    Task<ISOInfo?> DetectAsync(string isoPath, CancellationToken cancellationToken = default);
    VMConfiguration GetRecommendedConfiguration(ISOInfo isoInfo);
}

public record ISOInfo(
    string Path,
    OperatingSystem OS,
    long SizeBytes,
    bool IsBootable
);

public record VMConfiguration(
    string Name,
    int CpuCores,
    long MemoryMB,
    long DiskSizeGB,
    string? IsoPath,
    NetworkMode NetworkMode,
    OperatingSystem? DetectedOS = null,
    string? FloppyPath = null  // For older Windows that need driver floppies
);

public enum NetworkMode
{
    NAT,
    Bridge
}

public record OperatingSystem(
    string Name,
    string Version,
    OSType Type,
    string? Architecture = null  // e.g., "x86_64", "ARM64", "PowerPC"
);

public enum OSType
{
    Unknown,
    Windows,
    Linux,
    MacOS,
    BSD
}

public interface IISOLibraryManager
{
    void AddLibraryPath(string path);
    Task<IEnumerable<ISOInfo>> ScanLibraryAsync(CancellationToken cancellationToken = default);
    Task<ISOInfo?> GetISOInfoAsync(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<ISOInfo>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<ISOInfo>> GetByOSTypeAsync(OSType osType, CancellationToken cancellationToken = default);
    Task SaveCacheAsync(CancellationToken cancellationToken = default);
    Task LoadCacheAsync(CancellationToken cancellationToken = default);
    IEnumerable<string> GetLibraryPaths();
    int GetCachedCount();
}
