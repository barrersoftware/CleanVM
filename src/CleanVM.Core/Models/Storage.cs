namespace CleanVM.Core.Models;

public class Disk
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Path { get; init; }
    public long SizeBytes { get; set; }
    public DiskFormat Format { get; set; } = DiskFormat.Qcow2;
    public DiskBusType BusType { get; set; } = DiskBusType.VirtIO;
    public string? AttachedToVmId { get; set; }
    public DateTime CreatedAt { get; init; }
}

public class DiskCreateRequest
{
    public required string Name { get; set; }
    public long SizeBytes { get; set; }
    public DiskFormat Format { get; set; } = DiskFormat.Qcow2;
    public string? PoolId { get; set; }
}

public enum DiskFormat
{
    Raw,
    Qcow2,
    Vdi,
    Vmdk,
    Vhd
}

public enum DiskBusType
{
    IDE,
    SCSI,
    VirtIO,
    SATA
}

public class DiskStats
{
    public long UsedBytes { get; set; }
    public long AllocatedBytes { get; set; }
    public long CapacityBytes { get; set; }
    public long ReadOps { get; set; }
    public long WriteOps { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StoragePool
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Path { get; init; }
    public StoragePoolType Type { get; set; }
    public DateTime CreatedAt { get; init; }
}

public class StoragePoolCreateRequest
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public StoragePoolType Type { get; set; } = StoragePoolType.Directory;
}

public enum StoragePoolType
{
    Directory,
    LVM,
    ZFS,
    NFS
}

public class StoragePoolStats
{
    public long TotalBytes { get; set; }
    public long UsedBytes { get; set; }
    public long AvailableBytes { get; set; }
    public int DiskCount { get; set; }
}
