using CleanVM.Core.Hypervisor;
using CleanVM.Core.Services;
using CleanVM.Core.Storage;
using CleanVM.Core.Interfaces;
using CleanVM.Core.Models;

namespace CleanVM.Core.Tests.Integration;

/// <summary>
/// Full integration tests - creates REAL VMs with REAL libvirt/QEMU
/// These tests require libvirt and QEMU to be installed
/// </summary>
public class FullStackIntegrationTests : IDisposable
{
    private readonly string _testStoragePath;
    private readonly LibvirtHypervisorBackend _hypervisor;
    private readonly StorageManager _storageManager;
    private readonly VirtualMachineManager _vmManager;
    private readonly List<string> _createdVmIds = new();
    private readonly List<string> _createdDiskPaths = new();

    public FullStackIntegrationTests()
    {
        _testStoragePath = Path.Combine("/tmp", $"cleanvm-integration-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testStoragePath);

        _hypervisor = new LibvirtHypervisorBackend("qemu:///system", _testStoragePath);
        _storageManager = new StorageManager(_testStoragePath);
        _vmManager = new VirtualMachineManager(_hypervisor);
    }

    [Fact]
    public async Task StorageManager_ShouldCreateRealDisk()
    {
        // Create a real disk
        var request = new DiskCreateRequest
        {
            Name = "integration-test-disk",
            SizeBytes = 10 * 1024 * 1024, // 10MB
            Format = DiskFormat.Qcow2
        };

        var disk = await _storageManager.CreateDiskAsync(request);
        _createdDiskPaths.Add(disk.Path);

        // Verify disk file exists
        Assert.True(File.Exists(disk.Path), $"Disk should exist at {disk.Path}");
        
        // Verify it's a qcow2 file (check file signature)
        var bytes = await File.ReadAllBytesAsync(disk.Path);
        Assert.True(bytes.Length > 4);
        
        // qcow2 magic signature: 'Q', 'F', 'I', 0xfb
        Assert.Equal('Q', (char)bytes[0]);
        Assert.Equal('F', (char)bytes[1]);
        Assert.Equal('I', (char)bytes[2]);
        Assert.Equal(0xfb, bytes[3]);

        // Get disk stats
        var stats = await _storageManager.GetDiskStatsAsync(disk.Id);
        Assert.NotNull(stats);
        Assert.True(stats.CapacityBytes > 0);
    }

    [Fact]
    public async Task StorageManager_ShouldResizeDisk()
    {
        // Create disk
        var request = new DiskCreateRequest
        {
            Name = "resize-test-disk",
            SizeBytes = 10 * 1024 * 1024, // 10MB
            Format = DiskFormat.Qcow2
        };

        var disk = await _storageManager.CreateDiskAsync(request);
        _createdDiskPaths.Add(disk.Path);

        // Resize to 20MB
        await _storageManager.ResizeDiskAsync(disk.Id, 20 * 1024 * 1024);

        // Verify new size
        var updatedDisk = await _storageManager.GetDiskAsync(disk.Id);
        Assert.Equal(20 * 1024 * 1024, updatedDisk.SizeBytes);
    }

    [Fact]
    public async Task StorageManager_ShouldCloneDisk()
    {
        // Create original disk
        var request = new DiskCreateRequest
        {
            Name = "original-disk",
            SizeBytes = 10 * 1024 * 1024, // 10MB
            Format = DiskFormat.Qcow2
        };

        var originalDisk = await _storageManager.CreateDiskAsync(request);
        _createdDiskPaths.Add(originalDisk.Path);

        // Clone it
        await _storageManager.CloneDiskAsync(originalDisk.Id, "cloned-disk");

        // Verify both disks exist
        var disks = await _storageManager.ListDisksAsync();
        var clonedDisk = disks.FirstOrDefault(d => d.Name == "cloned-disk");
        
        Assert.NotNull(clonedDisk);
        Assert.True(File.Exists(clonedDisk.Path));
        _createdDiskPaths.Add(clonedDisk.Path);

        // Verify clone has same size
        Assert.Equal(originalDisk.SizeBytes, clonedDisk.SizeBytes);
    }

    public void Dispose()
    {
        // Clean up created VMs
        foreach (var vmId in _createdVmIds)
        {
            try
            {
                _hypervisor.DeleteVMAsync(vmId).Wait();
            }
            catch { }
        }

        // Clean up created disks
        foreach (var diskPath in _createdDiskPaths)
        {
            try
            {
                if (File.Exists(diskPath))
                    File.Delete(diskPath);
            }
            catch { }
        }

        // Clean up test storage directory
        try
        {
            if (Directory.Exists(_testStoragePath))
                Directory.Delete(_testStoragePath, true);
        }
        catch { }
    }
}
