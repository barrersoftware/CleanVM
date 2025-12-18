namespace CleanVM.Core.Models;

public class Iso
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Path { get; init; }
    public long SizeBytes { get; init; }
    public IsoMetadata? Metadata { get; set; }
    public DateTime CreatedAt { get; init; }
}

public class IsoImportRequest
{
    public required string SourcePath { get; set; }
    public string? Name { get; set; }
    public bool AutoDetect { get; set; } = true;
}

public class IsoMetadata
{
    public string? OperatingSystem { get; set; }
    public string? Version { get; set; }
    public string? Architecture { get; set; }
    public OsType Type { get; set; }
    public string? DisplayName { get; set; }
    public Dictionary<string, string> AdditionalInfo { get; set; } = new();
}

public enum OsType
{
    Unknown,
    Linux,
    Windows,
    MacOS,
    BSD,
    Other
}

public class IsoTemplate
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public required string DownloadUrl { get; set; }
    public long? SizeBytes { get; set; }
    public OsType Type { get; set; }
    public string? Version { get; set; }
    public VmRecommendedSettings? RecommendedSettings { get; set; }
}

public class VmRecommendedSettings
{
    public long MemoryMb { get; set; } = 2048;
    public int CpuCores { get; set; } = 2;
    public long DiskSizeGb { get; set; } = 20;
    public NetworkMode NetworkMode { get; set; } = NetworkMode.NAT;
    public string? Notes { get; set; }
}
