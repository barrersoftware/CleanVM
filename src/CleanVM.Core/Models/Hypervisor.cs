namespace CleanVM.Core.Models;

public class HypervisorCapabilities
{
    public bool SupportsVirtualization { get; set; }
    public bool SupportsSnapshots { get; set; }
    public bool SupportsHotplug { get; set; }
    public bool SupportsLiveMigration { get; set; }
    public List<string> SupportedDiskFormats { get; set; } = new();
    public List<string> SupportedNetworkTypes { get; set; } = new();
}

public class HostInfo
{
    public required string Hostname { get; set; }
    public required string Architecture { get; set; }
    public int CpuCores { get; set; }
    public int CpuThreads { get; set; }
    public long TotalMemoryBytes { get; set; }
    public string? CpuModel { get; set; }
    public string? HypervisorVersion { get; set; }
}

public class HostResources
{
    public int AvailableCpuCores { get; set; }
    public long AvailableMemoryBytes { get; set; }
    public long AvailableDiskBytes { get; set; }
    public int RunningVms { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
}

public class VmDefinition
{
    public required string Name { get; set; }
    public required string Id { get; set; }
    public long MemoryBytes { get; set; }
    public int CpuCores { get; set; }
    public List<VmDisk> Disks { get; set; } = new();
    public List<VmNetwork> Networks { get; set; } = new();
    public string? IsoPath { get; set; }
}

public class VmDisk
{
    public required string Path { get; set; }
    public string Device { get; set; } = "vda";
    public string Bus { get; set; } = "virtio";
}

public class VmNetwork
{
    public string Type { get; set; } = "network";
    public string Source { get; set; } = "default";
    public string Model { get; set; } = "virtio";
    public string? MacAddress { get; set; }
}

public class VmResources
{
    public long MemoryBytes { get; set; }
    public int CpuCores { get; set; }
}

public class VmMetrics
{
    public double CpuUsagePercent { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long DiskReadBytes { get; set; }
    public long DiskWriteBytes { get; set; }
    public long NetworkRxBytes { get; set; }
    public long NetworkTxBytes { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Snapshot
{
    public required string Id { get; init; }
    public required string VmId { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; init; }
}

public class ConsoleConnection
{
    public required string VmId { get; init; }
    public ConsoleType Type { get; set; }
    public required string ConnectionString { get; set; }
    public int? Port { get; set; }
}

public enum ConsoleType
{
    VNC,
    SPICE,
    Serial
}
