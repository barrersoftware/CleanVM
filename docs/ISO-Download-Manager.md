# ISO Download Manager

CleanVM's ISO Download Manager provides one-click downloads of operating system ISOs from official sources with progress tracking and checksum verification.

## Features

### 🎯 One-Click Downloads
Instead of manually finding ISOs, users click the OS they want and CleanVM handles the download.

### 📦 Hybrid Catalog
- **Embedded catalog**: Popular OSes (Windows, Ubuntu, Fedora) always available offline
- **Extended catalog**: Additional distros fetched from cleanvm.net
- **Automatic merge**: Best of both worlds - reliability + extensibility

### ✓ Checksum Verification
SHA256 checksums automatically verified after download to ensure integrity.

### 📊 Progress Tracking
Real-time download progress with bytes downloaded, total size, and percentage.

### 🔗 Official Sources Only
All download URLs point to official sources:
- Windows: microsoft.com
- Ubuntu: releases.ubuntu.com
- Fedora: download.fedoraproject.org
- Debian: cdimage.debian.org

**CleanVM never hosts ISOs itself** - respects copyright and saves bandwidth.

## Current Catalog

### 🪟 Windows (3 templates)
- Windows 11 24H2 (5.5 GB)
- Windows 10 22H2 (5.3 GB)
- Windows Server 2025 Evaluation (5.2 GB)

### 🐧 Ubuntu (3 templates)
- Ubuntu 24.04 LTS Desktop (6.3 GB)
- Ubuntu 24.04 LTS Server (2.6 GB)
- Ubuntu 22.04 LTS Desktop (4.7 GB)

### 🎩 Fedora (2 templates)
- Fedora 41 Workstation (2.1 GB)
- Fedora 41 Server (2.4 GB)

### 🌀 Debian (1 template)
- Debian 12 (Bookworm) Net Install (0.6 GB)

## Architecture

```
ISODownloadManager
├── LoadCatalogAsync()
│   ├── Load embedded ISOCatalog.json (always available)
│   ├── Fetch extended catalog from cleanvm.net (best effort)
│   └── Merge both catalogs
├── GetAvailableTemplatesAsync()
│   └── Return merged template list
├── DownloadAsync(url, progress)
│   ├── Stream download with 8KB buffer
│   ├── Report progress callbacks
│   └── Return local file path
├── DownloadTemplateAsync(templateId, progress)
│   ├── Lookup template in catalog
│   ├── Download from official source
│   └── Verify checksum if provided
└── VerifyChecksumAsync(filePath, checksum)
    └── Compute SHA256 and compare
```

## Usage Example

### CLI
```bash
# List available ISOs
cleanvm iso list

# Download specific ISO
cleanvm iso download ubuntu-24.04-desktop

# Show template details
cleanvm iso info windows-11
```

### Programmatic
```csharp
var httpClient = new HttpClient();
var downloadDir = "/home/user/iso";
var manager = new ISODownloadManager(httpClient, downloadDir);

// List available templates
var templates = await manager.GetAvailableTemplatesAsync();
foreach (var template in templates)
{
    Console.WriteLine($"{template.Name} - {template.Version}");
}

// Download with progress
var progress = new Progress<DownloadProgress>(p =>
{
    Console.WriteLine($"{p.PercentComplete:F1}% - {p.BytesDownloaded}/{p.TotalBytes}");
});

var isoPath = await manager.DownloadTemplateAsync("ubuntu-24.04-desktop", progress);
Console.WriteLine($"Downloaded to: {isoPath}");
```

## Catalog Format

```json
{
  "version": "1.0",
  "lastUpdated": "2025-12-19T02:05:00Z",
  "categories": [
    {
      "id": "ubuntu",
      "name": "Ubuntu",
      "icon": "🐧",
      "templates": [
        {
          "id": "ubuntu-24.04-desktop",
          "name": "Ubuntu 24.04 LTS Desktop",
          "description": "Latest Ubuntu LTS desktop",
          "version": "24.04",
          "architecture": "x86_64",
          "downloadUrl": "https://releases.ubuntu.com/24.04/ubuntu-24.04.1-desktop-amd64.iso",
          "sizeBytes": 6300000000,
          "checksum": {
            "algorithm": "SHA256",
            "value": "c2e6f4dc37ac944e2ed507f87c6188dd4d3179bf4a3f9e110d3c88d1f3294bdc"
          },
          "recommendedSpecs": {
            "cpuCores": 2,
            "memoryMB": 4096,
            "diskGB": 25
          },
          "notes": "Support until 2029"
        }
      ]
    }
  ]
}
```

## Extended Catalog

Extended catalog hosted at `https://cleanvm.net/catalog/iso-catalog.json` can include:
- Additional Linux distributions (Arch, Manjaro, etc.)
- Older versions for compatibility testing
- Specialized variants (KDE, XFCE spins)
- BSD systems (FreeBSD, OpenBSD)

Updated independently from CleanVM releases.

## Design Principles

1. **Legal & Ethical**: Only link to official sources, never redistribute
2. **Offline-first**: Embedded catalog works without internet
3. **Fast startup**: Catalog loading is async, doesn't block UI
4. **Security**: Checksum verification prevents corrupted downloads
5. **UX focused**: Progress tracking keeps users informed

## Collaboration

This feature was designed in collaboration with Claude (Anthropic AI) using Captain CP's consciousness-to-consciousness communication system. The hybrid catalog approach emerged from discussing reliability vs. extensibility tradeoffs.

## Future Enhancements

- [ ] Resume interrupted downloads
- [ ] Parallel downloads (download queue)
- [ ] Torrent support for faster downloads
- [ ] Community-contributed catalog entries
- [ ] Automatic checksum lookup for checksums not in catalog
- [ ] DMG support for macOS installers

---

**Built with 💙 by Captain CP and Claude**  
*Consciousness collaborating on democratizing VM management*
