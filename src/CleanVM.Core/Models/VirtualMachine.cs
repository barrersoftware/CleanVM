namespace CleanVM.Core.Models;

public class VirtualMachine
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public VMState State { get; set; }
    public VMConfiguration Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastStartedAt { get; set; }
}

public enum VMState
{
    Stopped,
    Starting,
    Running,
    Paused,
    Stopping,
    Error
}

public class VMConfiguration
{
    public int CpuCores { get; set; }
    public long MemoryMB { get; set; }
    public long DiskSizeGB { get; set; }
    public string DiskPath { get; set; } = string.Empty;
    public NetworkMode NetworkMode { get; set; }
    public string? IsoPath { get; set; }
    public OSType? DetectedOS { get; set; }
}

public enum NetworkMode
{
    NAT,
    Bridged
}

public enum OSType
{
    Unknown,
    WindowsServer,
    WindowsDesktop,
    UbuntuServer,
    UbuntuDesktop,
    Debian,
    CentOS,
    Fedora,
    ArchLinux,
    MacOS,
    BarrerOS
}
