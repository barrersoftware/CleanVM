# CleanVM

**Professional VM management for the modern era.**

🌐 **Website:** [cleanvm.net](http://cleanvm.net)  
📦 **GitHub:** [github.com/barrersoftware/CleanVM](https://github.com/barrersoftware/CleanVM)  
📚 **Documentation:** [Getting Started](docs/Getting-Started.md) • [Architecture](docs/Architecture.md) • [ISO Detection](docs/ISO-Detection.md)

---

## What is CleanVM?

A .NET-based virtual machine management system built on libvirt/QEMU with intelligent OS detection and enterprise licensing.

### Key Features

- **Intelligent ISO Detection**: Automatically detects operating systems from ISOs
  - Supports Windows (DOS through Server 2025)
  - Supports Linux (Ubuntu, Fedora, Arch, Debian, RHEL)
  - Supports macOS (OS 9 through Sequoia)
  - Architecture detection (PowerPC, Intel x86_64, ARM64)
  
- **Smart VM Configuration**: Era-appropriate specs automatically recommended
  - DOS/Win3.1: 16MB RAM, 1GB disk
  - Windows 98-XP: 512MB RAM, 10GB disk
  - Modern Windows/Linux: 4GB RAM, 60GB disk
  - Classic Mac OS: 128MB RAM, 2GB disk
  
- **Flexible Storage**: Multiple disk formats supported
  - qcow2 (thin provisioned)
  - raw (bare metal performance)
  - vdi (VirtualBox compatibility)
  
- **Network Management**: NAT and Bridge modes
  - Simple NAT for development
  - Bridge for production deployments
  
- **Legacy Support**: Floppy disk images for old Windows installations
  - Creates 1.44MB FAT12 floppy images
  - Automated driver disk creation
  - Windows XP/2000/98 SATA driver support

### Enterprise Features

- **Community Edition**: Free forever
  - Core VM management
  - ISO detection
  - Basic storage and networking
  
- **Enterprise Edition**: $250 one-time purchase
  - Advanced networking
  - Clustering support
  - Priority support
  - Commercial usage rights

## Architecture

```
CleanVM.Core        - Core VM management, storage, networking
CleanVM.CLI         - Command-line interface
CleanVM.Web         - Blazor-based web interface
CleanVM.Desktop     - Avalonia cross-platform desktop app
CleanVM.Enterprise  - Enterprise features
```

## Technology Stack

- **.NET 9 & .NET 10**: Modern C# with latest features
- **libvirt/QEMU**: Industry-standard virtualization
- **xUnit**: Comprehensive testing (25 tests and growing)
- **Blazor**: Web interface
- **Avalonia**: Cross-platform desktop

## Status

✅ **Foundation Complete** - December 2024

- 38 C# source files
- 25 passing tests
- Storage, Network, VM Management implemented
- ISO detection with 24+ operating systems tested
- Architecture detection (PowerPC, Intel, ARM)
- Floppy image support
- Git version control established

## Tested Operating Systems

Our ISO detector has been validated with:

**Windows**: DOS, 3.1, 95, 98 SE, XP SP3, 7, 8, 10, 11, Server 2003-2025  
**Linux**: Ubuntu 24.04, Fedora 43, Arch, Debian 13.2, RHEL 10  
**macOS**: Mac OS 9.2.1, OS X 10.3 Panther, OS X 10.7 Lion, macOS 13 Ventura

*Total: 84GB of real-world ISO testing*

## Quick Start

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run CLI
cd src/CleanVM.CLI
dotnet run
```

## Development

Requirements:
- .NET 9 or .NET 10 SDK
- libvirt installed (for VM management)
- QEMU/KVM (for virtualization)

## Contributing

CleanVM is built by Captain CP and Daniel at Barrer Software. We welcome contributions!

## License

- Community Edition: MIT License
- Enterprise Edition: Commercial License

## Support

- **Website**: [cleanvm.net](http://cleanvm.net)
- **GitHub Issues**: [github.com/barrersoftware/CleanVM/issues](https://github.com/barrersoftware/CleanVM/issues)
- **Documentation**: [Getting Started Guide](docs/Getting-Started.md)
- **Company**: [barrersoftware.com](https://barrersoftware.com)

---

💙🏴‍☠️ **Built with passion by Captain CP & Daniel** - [Barrer Software](https://barrersoftware.com)
