using CleanVM.Core.Interfaces;
using CleanVM.Core.Models;
using System.Diagnostics;

namespace CleanVM.Core.Storage;

/// <summary>
/// Manages virtual disks, storage pools, and disk operations
/// </summary>
public class StorageManager : IStorageManager
{
    private readonly Dictionary<string, Disk> _disks = new();
    private readonly Dictionary<string, StoragePool> _pools = new();
    private readonly string _defaultPoolPath;

    public StorageManager(string? defaultPoolPath = null)
    {
        _defaultPoolPath = defaultPoolPath ?? "/var/lib/cleanvm/storage";
        
        // Create default pool
        if (!Directory.Exists(_defaultPoolPath))
            Directory.CreateDirectory(_defaultPoolPath);
    }

    public async Task<Disk> CreateDiskAsync(DiskCreateRequest request, CancellationToken cancellationToken = default)
    {
        var diskId = Guid.NewGuid().ToString();
        var poolPath = string.IsNullOrEmpty(request.PoolId) 
            ? _defaultPoolPath 
            : _pools[request.PoolId].Path;
        
        var extension = request.Format switch
        {
            DiskFormat.Raw => "raw",
            DiskFormat.Qcow2 => "qcow2",
            DiskFormat.Vdi => "vdi",
            DiskFormat.Vmdk => "vmdk",
            DiskFormat.Vhd => "vhd",
            _ => "qcow2"
        };
        
        var diskPath = Path.Combine(poolPath, $"{diskId}.{extension}");
        
        // Create disk image using qemu-img
        await CreateDiskImageAsync(diskPath, request.SizeBytes, request.Format, cancellationToken);
        
        var disk = new Disk
        {
            Id = diskId,
            Name = request.Name,
            Path = diskPath,
            SizeBytes = request.SizeBytes,
            Format = request.Format,
            CreatedAt = DateTime.UtcNow
        };
        
        _disks[diskId] = disk;
        return disk;
    }

    public Task<Disk> GetDiskAsync(string diskId, CancellationToken cancellationToken = default)
    {
        if (!_disks.TryGetValue(diskId, out var disk))
            throw new KeyNotFoundException($"Disk {diskId} not found");
        return Task.FromResult(disk);
    }

    public Task<IEnumerable<Disk>> ListDisksAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Disk>>(_disks.Values);
    }

    public async Task DeleteDiskAsync(string diskId, CancellationToken cancellationToken = default)
    {
        var disk = await GetDiskAsync(diskId, cancellationToken);
        
        if (!string.IsNullOrEmpty(disk.AttachedToVmId))
            throw new InvalidOperationException($"Disk {diskId} is attached to VM {disk.AttachedToVmId}");
        
        if (File.Exists(disk.Path))
            File.Delete(disk.Path);
        
        _disks.Remove(diskId);
    }

    public async Task ResizeDiskAsync(string diskId, long newSizeBytes, CancellationToken cancellationToken = default)
    {
        var disk = await GetDiskAsync(diskId, cancellationToken);
        
        if (newSizeBytes < disk.SizeBytes)
            throw new InvalidOperationException("Cannot shrink disks");
        
        // Use qemu-img resize
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "qemu-img",
                Arguments = $"resize {disk.Path} {newSizeBytes}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode == 0)
            disk.SizeBytes = newSizeBytes;
        else
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"Failed to resize disk: {error}");
        }
    }

    public async Task CloneDiskAsync(string diskId, string newDiskName, CancellationToken cancellationToken = default)
    {
        var sourceDisk = await GetDiskAsync(diskId, cancellationToken);
        var newDiskId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(sourceDisk.Path);
        var newPath = Path.Combine(Path.GetDirectoryName(sourceDisk.Path)!, $"{newDiskId}{extension}");
        
        // Clone using qemu-img convert
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "qemu-img",
                Arguments = $"convert -O {sourceDisk.Format.ToString().ToLower()} {sourceDisk.Path} {newPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"Failed to clone disk: {error}");
        }
        
        var newDisk = new Disk
        {
            Id = newDiskId,
            Name = newDiskName,
            Path = newPath,
            SizeBytes = sourceDisk.SizeBytes,
            Format = sourceDisk.Format,
            BusType = sourceDisk.BusType,
            CreatedAt = DateTime.UtcNow
        };
        
        _disks[newDiskId] = newDisk;
    }

    public async Task<DiskStats> GetDiskStatsAsync(string diskId, CancellationToken cancellationToken = default)
    {
        var disk = await GetDiskAsync(diskId, cancellationToken);
        
        // Get disk info using qemu-img info
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "qemu-img",
                Arguments = $"info --output=json {disk.Path}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        // Parse JSON output (simplified - would use System.Text.Json in production)
        return new DiskStats
        {
            CapacityBytes = disk.SizeBytes,
            AllocatedBytes = disk.SizeBytes, // Simplified
            UsedBytes = 0, // Would parse from JSON
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task AttachDiskToVmAsync(string vmId, string diskId, CancellationToken cancellationToken = default)
    {
        var disk = await GetDiskAsync(diskId, cancellationToken);
        
        if (!string.IsNullOrEmpty(disk.AttachedToVmId))
            throw new InvalidOperationException($"Disk is already attached to VM {disk.AttachedToVmId}");
        
        disk.AttachedToVmId = vmId;
    }

    public async Task DetachDiskFromVmAsync(string vmId, string diskId, CancellationToken cancellationToken = default)
    {
        var disk = await GetDiskAsync(diskId, cancellationToken);
        
        if (disk.AttachedToVmId != vmId)
            throw new InvalidOperationException($"Disk is not attached to VM {vmId}");
        
        disk.AttachedToVmId = null;
    }

    public Task<IEnumerable<Disk>> GetVmDisksAsync(string vmId, CancellationToken cancellationToken = default)
    {
        var vmDisks = _disks.Values.Where(d => d.AttachedToVmId == vmId);
        return Task.FromResult(vmDisks);
    }

    public Task<StoragePool> CreatePoolAsync(StoragePoolCreateRequest request, CancellationToken cancellationToken = default)
    {
        var poolId = Guid.NewGuid().ToString();
        
        if (!Directory.Exists(request.Path))
            Directory.CreateDirectory(request.Path);
        
        var pool = new StoragePool
        {
            Id = poolId,
            Name = request.Name,
            Path = request.Path,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };
        
        _pools[poolId] = pool;
        return Task.FromResult(pool);
    }

    public Task<IEnumerable<StoragePool>> ListPoolsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<StoragePool>>(_pools.Values);
    }

    public Task<StoragePoolStats> GetPoolStatsAsync(string poolId, CancellationToken cancellationToken = default)
    {
        if (!_pools.TryGetValue(poolId, out var pool))
            throw new KeyNotFoundException($"Pool {poolId} not found");
        
        var driveInfo = new DriveInfo(pool.Path);
        var disksInPool = _disks.Values.Where(d => d.Path.StartsWith(pool.Path)).Count();
        
        return Task.FromResult(new StoragePoolStats
        {
            TotalBytes = driveInfo.TotalSize,
            UsedBytes = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
            AvailableBytes = driveInfo.AvailableFreeSpace,
            DiskCount = disksInPool
        });
    }

    public Task DeletePoolAsync(string poolId, CancellationToken cancellationToken = default)
    {
        if (!_pools.TryGetValue(poolId, out var pool))
            throw new KeyNotFoundException($"Pool {poolId} not found");
        
        var disksInPool = _disks.Values.Where(d => d.Path.StartsWith(pool.Path));
        if (disksInPool.Any())
            throw new InvalidOperationException("Cannot delete pool with existing disks");
        
        _pools.Remove(poolId);
        return Task.CompletedTask;
    }

    private async Task CreateDiskImageAsync(string path, long sizeBytes, DiskFormat format, CancellationToken cancellationToken)
    {
        var formatStr = format switch
        {
            DiskFormat.Raw => "raw",
            DiskFormat.Qcow2 => "qcow2",
            DiskFormat.Vdi => "vdi",
            DiskFormat.Vmdk => "vmdk",
            DiskFormat.Vhd => "vpc",
            _ => "qcow2"
        };

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "qemu-img",
                Arguments = $"create -f {formatStr} {path} {sizeBytes}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"Failed to create disk image: {error}");
        }
    }
}
