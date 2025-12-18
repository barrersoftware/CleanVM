using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface IHypervisorProvider
{
    // Hypervisor Info
    string Name { get; }
    string Version { get; }
    HypervisorCapabilities Capabilities { get; }
    
    // Connection
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<bool> IsConnectedAsync();
    
    // Host Information
    Task<HostInfo> GetHostInfoAsync(CancellationToken cancellationToken = default);
    Task<HostResources> GetAvailableResourcesAsync(CancellationToken cancellationToken = default);
    
    // VM Operations (Low-level)
    Task<string> CreateVmAsync(VmDefinition definition, CancellationToken cancellationToken = default);
    Task StartVmAsync(string vmId, CancellationToken cancellationToken = default);
    Task StopVmAsync(string vmId, bool force, CancellationToken cancellationToken = default);
    Task DestroyVmAsync(string vmId, CancellationToken cancellationToken = default);
    
    // Resource Management
    Task<VmResources> GetVmResourcesAsync(string vmId, CancellationToken cancellationToken = default);
    Task UpdateVmResourcesAsync(string vmId, VmResources resources, CancellationToken cancellationToken = default);
    
    // Monitoring
    Task<VmMetrics> GetVmMetricsAsync(string vmId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<VmMetrics> StreamVmMetricsAsync(string vmId, CancellationToken cancellationToken = default);
}
