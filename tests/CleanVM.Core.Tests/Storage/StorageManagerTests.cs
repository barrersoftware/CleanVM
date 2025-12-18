using CleanVM.Core.Storage;
using CleanVM.Core.Models;

namespace CleanVM.Core.Tests.Storage;

public class StorageManagerTests
{
    private readonly string _testPath;
    private readonly StorageManager _storageManager;

    public StorageManagerTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "cleanvm-test-storage", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPath);
        _storageManager = new StorageManager(_testPath);
    }

    [Fact]
    public async Task CreateDisk_ShouldCreateQcow2Disk()
    {
        // Arrange
        var request = new DiskCreateRequest
        {
            Name = "test-disk",
            SizeBytes = 1024 * 1024 * 100, // 100MB
            Format = DiskFormat.Qcow2
        };

        // Act
        var disk = await _storageManager.CreateDiskAsync(request);

        // Assert
        Assert.NotNull(disk);
        Assert.Equal("test-disk", disk.Name);
        Assert.Equal(DiskFormat.Qcow2, disk.Format);
        Assert.True(File.Exists(disk.Path), $"Disk file should exist at {disk.Path}");
        
        // Cleanup
        if (File.Exists(disk.Path))
            File.Delete(disk.Path);
    }

    [Fact]
    public async Task CreateDisk_ShouldCreateRawDisk()
    {
        // Arrange
        var request = new DiskCreateRequest
        {
            Name = "test-raw-disk",
            SizeBytes = 1024 * 1024 * 50, // 50MB
            Format = DiskFormat.Raw
        };

        // Act
        var disk = await _storageManager.CreateDiskAsync(request);

        // Assert
        Assert.NotNull(disk);
        Assert.Equal(DiskFormat.Raw, disk.Format);
        Assert.True(File.Exists(disk.Path));
        
        // Cleanup
        if (File.Exists(disk.Path))
            File.Delete(disk.Path);
    }

    [Fact]
    public async Task ListDisks_ShouldReturnAllDisks()
    {
        // Arrange
        var request1 = new DiskCreateRequest { Name = "disk1", SizeBytes = 1024 * 1024 * 10, Format = DiskFormat.Qcow2 };
        var request2 = new DiskCreateRequest { Name = "disk2", SizeBytes = 1024 * 1024 * 10, Format = DiskFormat.Qcow2 };

        var disk1 = await _storageManager.CreateDiskAsync(request1);
        var disk2 = await _storageManager.CreateDiskAsync(request2);

        // Act
        var disks = await _storageManager.ListDisksAsync();

        // Assert
        Assert.Contains(disks, d => d.Id == disk1.Id);
        Assert.Contains(disks, d => d.Id == disk2.Id);
        
        // Cleanup
        foreach (var disk in disks)
        {
            if (File.Exists(disk.Path))
                File.Delete(disk.Path);
        }
    }

    [Fact]
    public async Task DeleteDisk_ShouldRemoveDisk()
    {
        // Arrange
        var request = new DiskCreateRequest { Name = "delete-test", SizeBytes = 1024 * 1024 * 10, Format = DiskFormat.Qcow2 };
        var disk = await _storageManager.CreateDiskAsync(request);
        var diskPath = disk.Path;

        // Act
        await _storageManager.DeleteDiskAsync(disk.Id);

        // Assert
        Assert.False(File.Exists(diskPath), "Disk file should be deleted");
        
        var disks = await _storageManager.ListDisksAsync();
        Assert.DoesNotContain(disks, d => d.Id == disk.Id);
    }

    [Fact]
    public async Task AttachDiskToVm_ShouldSetVmId()
    {
        // Arrange
        var request = new DiskCreateRequest { Name = "attach-test", SizeBytes = 1024 * 1024 * 10, Format = DiskFormat.Qcow2 };
        var disk = await _storageManager.CreateDiskAsync(request);
        var vmId = "test-vm-123";

        // Act
        await _storageManager.AttachDiskToVmAsync(vmId, disk.Id);

        // Assert
        var updatedDisk = await _storageManager.GetDiskAsync(disk.Id);
        Assert.Equal(vmId, updatedDisk.AttachedToVmId);
        
        // Cleanup
        if (File.Exists(disk.Path))
            File.Delete(disk.Path);
    }

    [Fact]
    public async Task DetachDiskFromVm_ShouldClearVmId()
    {
        // Arrange
        var request = new DiskCreateRequest { Name = "detach-test", SizeBytes = 1024 * 1024 * 10, Format = DiskFormat.Qcow2 };
        var disk = await _storageManager.CreateDiskAsync(request);
        var vmId = "test-vm-456";
        await _storageManager.AttachDiskToVmAsync(vmId, disk.Id);

        // Act
        await _storageManager.DetachDiskFromVmAsync(vmId, disk.Id);

        // Assert
        var updatedDisk = await _storageManager.GetDiskAsync(disk.Id);
        Assert.Null(updatedDisk.AttachedToVmId);
        
        // Cleanup
        if (File.Exists(disk.Path))
            File.Delete(disk.Path);
    }

    [Fact]
    public async Task CreateStoragePool_ShouldCreatePool()
    {
        // Arrange
        var poolPath = Path.Combine(_testPath, "test-pool");
        var request = new StoragePoolCreateRequest
        {
            Name = "test-pool",
            Path = poolPath,
            Type = StoragePoolType.Directory
        };

        // Act
        var pool = await _storageManager.CreatePoolAsync(request);

        // Assert
        Assert.NotNull(pool);
        Assert.Equal("test-pool", pool.Name);
        Assert.True(Directory.Exists(poolPath));
        
        // Cleanup
        if (Directory.Exists(poolPath))
            Directory.Delete(poolPath, true);
    }
}
