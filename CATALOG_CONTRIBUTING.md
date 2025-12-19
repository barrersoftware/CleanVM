# Contributing to CleanVM ISO Catalog

The CleanVM ISO catalog (`catalog.json`) is **community-maintained**. Anyone can submit updates!

## How to Add or Update an ISO

1. **Fork the repository**
2. **Edit `catalog.json`** in the root directory
3. **Add or update an ISO template** following the format below
4. **Submit a Pull Request**

## ISO Template Format

```json
{
  "id": "unique-id-here",
  "name": "OS Name Version",
  "description": "Brief description",
  "version": "version-number",
  "architecture": "x86_64",
  "downloadUrl": "https://official-source.com/path/to.iso",
  "sizeBytes": 1234567890,
  "checksum": {
    "algorithm": "SHA256",
    "value": "sha256-hash-here"
  },
  "recommendedSpecs": {
    "cpuCores": 2,
    "memoryMB": 4096,
    "diskGB": 25
  },
  "notes": "Any important notes"
}
```

## Guidelines

### ✅ DO
- **Link to official sources only** (vendor websites, official mirrors)
- **Include SHA256 checksums** when available
- **Use accurate version numbers**
- **Test download URLs** before submitting
- **Keep descriptions clear and concise**
- **Add new categories** if needed (with appropriate icon emoji)

### ❌ DON'T
- **Never host ISOs ourselves** - only link to official sources
- **Don't link to unofficial/third-party mirrors**
- **Don't include pirated or license-violating content**
- **Don't add EOL versions** unless specifically needed
- **Don't guess checksums** - leave empty if unknown

## Categories

Current categories:
- 🪟 **Windows** - Windows desktop and server editions
- 🐧 **Ubuntu** - Ubuntu desktop and server
- 🎩 **Fedora** - Fedora workstation and server
- 🌀 **Debian** - Debian stable releases
- Add more as needed!

## Example: Adding Rocky Linux

```json
{
  "id": "rockylinux",
  "name": "Rocky Linux",
  "icon": "🪨",
  "templates": [
    {
      "id": "rocky-9.3-minimal",
      "name": "Rocky Linux 9.3 Minimal",
      "description": "Enterprise Linux compatible minimal install",
      "version": "9.3",
      "architecture": "x86_64",
      "downloadUrl": "https://download.rockylinux.org/pub/rocky/9/isos/x86_64/Rocky-9.3-x86_64-minimal.iso",
      "sizeBytes": 2100000000,
      "checksum": {
        "algorithm": "SHA256",
        "value": "actual-checksum-here"
      },
      "recommendedSpecs": {
        "cpuCores": 2,
        "memoryMB": 2048,
        "diskGB": 20
      },
      "notes": "RHEL-compatible, 10-year support"
    }
  ]
}
```

## Verification

Your PR will be automatically checked for:
- Valid JSON syntax
- Required fields present
- URLs are accessible
- Checksums match (if provided)

## Questions?

Open an issue or ask in discussions!

---

**🏴‍☠️ Built with CleanVM - Democratizing virtualization**
