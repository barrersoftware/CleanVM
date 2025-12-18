# Getting Started with CleanVM

## Prerequisites

- .NET 9 or .NET 10 SDK
- libvirt installed and running
- QEMU/KVM for virtualization
- Linux host (tested on Ubuntu/Debian/Fedora)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/barrersoftware/CleanVM.git
cd CleanVM
```

### 2. Install Dependencies

**Ubuntu/Debian:**
```bash
sudo apt install qemu-kvm libvirt-daemon-system libvirt-clients bridge-utils
sudo systemctl start libvirtd
sudo systemctl enable libvirtd
```

**Fedora/RHEL:**
```bash
sudo dnf install qemu-kvm libvirt virt-install bridge-utils
sudo systemctl start libvirtd
sudo systemctl enable libvirtd
```

### 3. Build CleanVM

```bash
dotnet restore
dotnet build
```

### 4. Run Tests

```bash
dotnet test
```

## Your First VM

### Using the CLI

```bash
cd src/CleanVM.CLI
dotnet run
```

### Detecting an ISO

CleanVM can automatically detect operating systems from ISO files:

```csharp
using CleanVM.Core.Services;

var detector = new ISODetector();
var isoInfo = await detector.DetectAsync("/path/to/ubuntu-24.04.iso");

Console.WriteLine($"OS: {isoInfo.OS.Name} {isoInfo.OS.Version}");
Console.WriteLine($"Architecture: {isoInfo.OS.Architecture}");
Console.WriteLine($"Bootable: {isoInfo.IsBootable}");

var config = detector.GetRecommendedConfiguration(isoInfo);
Console.WriteLine($"Recommended: {config.CpuCores} CPU, {config.MemoryMB}MB RAM");
```

### Creating a VM

```csharp
using CleanVM.Core.Services;
using CleanVM.Core.Storage;

var storageManager = new StorageManager();
var vmManager = new VirtualMachineManager();

// Create disk
var disk = await storageManager.CreateDiskAsync(
    "/var/lib/libvirt/images/myvm.qcow2",
    sizeGB: 20,
    format: DiskFormat.Qcow2
);

// Create VM configuration
var vm = new VMConfiguration(
    Name: "MyVM",
    CpuCores: 2,
    MemoryMB: 2048,
    DiskSizeGB: 20,
    IsoPath: "/path/to/os.iso",
    NetworkMode: NetworkMode.NAT,
    DetectedOS: null
);

// Create and start VM
await vmManager.CreateAsync(vm);
await vmManager.StartAsync("MyVM");
```

## Supported Operating Systems

CleanVM has been tested with:

- **Windows**: DOS, 3.1, 95, 98, XP, 7, 8, 10, 11, Server 2003-2025
- **Linux**: Ubuntu, Fedora, Arch, Debian, RHEL, CentOS
- **macOS**: Mac OS 9, OS X 10.3-10.7, macOS 11-15

## Next Steps

- Read the [Architecture Guide](Architecture.md)
- Learn about [ISO Detection](ISO-Detection.md)
- Explore [Storage Management](Storage.md)
- Configure [Networking](Networking.md)
