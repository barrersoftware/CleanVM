# CleanVM Architecture

## Overview

CleanVM is built as a modular .NET application with clear separation of concerns.

```
┌─────────────────────────────────────────┐
│         User Interfaces                 │
│  ┌──────────┬──────────┬─────────────┐ │
│  │ CLI      │ Web UI   │ Desktop App │ │
│  │ (Console)│ (Blazor) │ (Avalonia)  │ │
│  └──────────┴──────────┴─────────────┘ │
└─────────────────────────────────────────┘
              ▲
              │
┌─────────────────────────────────────────┐
│         CleanVM.Core                    │
│  ┌─────────────────────────────────┐   │
│  │ Services                        │   │
│  │  • VirtualMachineManager        │   │
│  │  • ISODetector                  │   │
│  │  • ISODeepInspector             │   │
│  │  • ISOLibraryManager            │   │
│  │  • LicenseManager               │   │
│  └─────────────────────────────────┘   │
│  ┌─────────────────────────────────┐   │
│  │ Storage                         │   │
│  │  • StorageManager               │   │
│  │  • FloppyImageManager           │   │
│  └─────────────────────────────────┘   │
│  ┌─────────────────────────────────┐   │
│  │ Network                         │   │
│  │  • NetworkManager               │   │
│  │  • NAT / Bridge support         │   │
│  └─────────────────────────────────┘   │
│  ┌─────────────────────────────────┐   │
│  │ Hypervisor                      │   │
│  │  • LibvirtHypervisorBackend     │   │
│  └─────────────────────────────────┘   │
└─────────────────────────────────────────┘
              ▲
              │
┌─────────────────────────────────────────┐
│      libvirt / QEMU / KVM               │
└─────────────────────────────────────────┘
```

## Core Components

### VirtualMachineManager
Manages VM lifecycle: creation, starting, stopping, deletion.

### ISODetector
Intelligent OS detection from ISO files:
- Reads ISO9660 volume labels
- Parses El Torito boot records
- Detects OS type, version, and architecture
- Recommends era-appropriate specs

### StorageManager
Handles disk creation and management:
- Multiple formats (qcow2, raw, vdi)
- Thin provisioning support
- Disk resizing
- Snapshot management

### NetworkManager
Manages VM networking:
- NAT mode for development
- Bridge mode for production
- Automatic network configuration

### FloppyImageManager
Creates floppy disk images for legacy systems:
- 1.44MB FAT12 format
- Automated driver disk creation
- Windows XP/2000/98 SATA driver support

## Design Principles

### 1. Separation of Concerns
Each component has a single, well-defined responsibility.

### 2. Interface-Based Design
All major components implement interfaces for testability and flexibility.

### 3. Asynchronous by Default
All I/O operations are async to avoid blocking.

### 4. Era-Appropriate Defaults
The system understands that DOS doesn't need 4GB RAM.

### 5. Zero Configuration
Works out of the box with sensible defaults.

## Technology Stack

- **Language**: C# 12 / .NET 9 & 10
- **Virtualization**: libvirt + QEMU/KVM
- **Testing**: xUnit
- **Web UI**: Blazor Server
- **Desktop UI**: Avalonia
- **Build**: MSBuild / dotnet CLI

## Extension Points

### Custom Hypervisor Backends
Implement `IHypervisorBackend` to support other hypervisors:
```csharp
public interface IHypervisorBackend
{
    Task<bool> IsInstalledAsync();
    Task<string> CreateVMAsync(VMConfiguration config);
    Task StartVMAsync(string vmId);
    Task StopVMAsync(string vmId);
}
```

### Custom Storage Providers
Implement `IStorageManager` for alternative storage:
```csharp
public interface IStorageManager
{
    Task<DiskInfo> CreateDiskAsync(string path, int sizeGB, DiskFormat format);
    Task<bool> DiskExistsAsync(string path);
    Task DeleteDiskAsync(string path);
}
```

## Performance Characteristics

- **ISO Detection**: <1 second per ISO
- **VM Creation**: 2-5 seconds
- **Disk Creation**: Depends on size (thin provisioning is instant)
- **Memory Usage**: <100MB for core library

## Security Considerations

- All VM operations require appropriate system permissions
- Enterprise edition includes license validation
- No telemetry or external connections (privacy-first)
