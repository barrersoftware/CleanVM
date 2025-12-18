namespace CleanVM.Core.Models;

public class Network
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public NetworkType Type { get; set; }
    public string? BridgeName { get; set; }
    public string? Subnet { get; set; }
    public bool EnableDhcp { get; set; } = true;
    public DateTime CreatedAt { get; init; }
}

public class NetworkCreateRequest
{
    public required string Name { get; set; }
    public NetworkType Type { get; set; } = NetworkType.NAT;
    public string? BridgeName { get; set; }
    public string? Subnet { get; set; }
    public bool EnableDhcp { get; set; } = true;
}

public enum NetworkType
{
    NAT,
    Bridge,
    HostOnly,
    Internal
}

public class NetworkInterface
{
    public required string Id { get; init; }
    public required string VmId { get; init; }
    public required string NetworkId { get; init; }
    public string? MacAddress { get; set; }
    public string? IpAddress { get; set; }
    public NetworkInterfaceModel Model { get; set; } = NetworkInterfaceModel.VirtIO;
}

public class NetworkInterfaceCreateRequest
{
    public required string NetworkId { get; set; }
    public string? MacAddress { get; set; }
    public NetworkInterfaceModel Model { get; set; } = NetworkInterfaceModel.VirtIO;
}

public enum NetworkInterfaceModel
{
    E1000,
    VirtIO,
    RTL8139
}

public class NetworkStats
{
    public long RxBytes { get; set; }
    public long TxBytes { get; set; }
    public long RxPackets { get; set; }
    public long TxPackets { get; set; }
    public int ConnectedVms { get; set; }
    public DateTime Timestamp { get; set; }
}

public class NetworkInterfaceStats
{
    public long RxBytes { get; set; }
    public long TxBytes { get; set; }
    public long RxPackets { get; set; }
    public long TxPackets { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DhcpLease
{
    public required string VmId { get; init; }
    public required string IpAddress { get; init; }
    public required string MacAddress { get; init; }
    public DateTime LeaseStart { get; init; }
    public DateTime LeaseEnd { get; init; }
    public string? Hostname { get; set; }
}
