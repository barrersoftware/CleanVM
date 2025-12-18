using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface INetworkManager
{
    // Network Management
    Task<Models.Network> CreateNetworkAsync(NetworkCreateRequest request, CancellationToken cancellationToken = default);
    Task<Models.Network> GetNetworkAsync(string networkId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Network>> ListNetworksAsync(CancellationToken cancellationToken = default);
    Task DeleteNetworkAsync(string networkId, CancellationToken cancellationToken = default);
    
    // Network Interfaces
    Task<NetworkInterface> CreateInterfaceAsync(string vmId, NetworkInterfaceCreateRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<NetworkInterface>> GetVmInterfacesAsync(string vmId, CancellationToken cancellationToken = default);
    Task DeleteInterfaceAsync(string vmId, string interfaceId, CancellationToken cancellationToken = default);
    
    // Network Operations
    Task AttachInterfaceAsync(string vmId, string networkId, CancellationToken cancellationToken = default);
    Task DetachInterfaceAsync(string vmId, string interfaceId, CancellationToken cancellationToken = default);
    Task<NetworkStats> GetNetworkStatsAsync(string networkId, CancellationToken cancellationToken = default);
    Task<NetworkInterfaceStats> GetInterfaceStatsAsync(string vmId, string interfaceId, CancellationToken cancellationToken = default);
    
    // DHCP & DNS
    Task<DhcpLease> GetDhcpLeaseAsync(string vmId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DhcpLease>> ListDhcpLeasesAsync(string networkId, CancellationToken cancellationToken = default);
}
