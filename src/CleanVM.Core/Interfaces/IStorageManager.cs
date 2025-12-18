using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface IStorageManager
{
    // Disk Management
    Task<Disk> CreateDiskAsync(DiskCreateRequest request, CancellationToken cancellationToken = default);
    Task<Disk> GetDiskAsync(string diskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Disk>> ListDisksAsync(CancellationToken cancellationToken = default);
    Task DeleteDiskAsync(string diskId, CancellationToken cancellationToken = default);
    
    // Disk Operations
    Task ResizeDiskAsync(string diskId, long newSizeBytes, CancellationToken cancellationToken = default);
    Task CloneDiskAsync(string diskId, string newDiskName, CancellationToken cancellationToken = default);
    Task<DiskStats> GetDiskStatsAsync(string diskId, CancellationToken cancellationToken = default);
    
    // Disk Attachment
    Task AttachDiskToVmAsync(string vmId, string diskId, CancellationToken cancellationToken = default);
    Task DetachDiskFromVmAsync(string vmId, string diskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Disk>> GetVmDisksAsync(string vmId, CancellationToken cancellationToken = default);
    
    // Storage Pools
    Task<StoragePool> CreatePoolAsync(StoragePoolCreateRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<StoragePool>> ListPoolsAsync(CancellationToken cancellationToken = default);
    Task<StoragePoolStats> GetPoolStatsAsync(string poolId, CancellationToken cancellationToken = default);
    Task DeletePoolAsync(string poolId, CancellationToken cancellationToken = default);
}
