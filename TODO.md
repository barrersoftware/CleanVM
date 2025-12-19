# CleanVM TODO - Implementation Checklist

Based on our documentation promises vs actual implementation.

## ✅ COMPLETED
- [x] Project structure and architecture
- [x] Core interfaces defined
- [x] ISO Detection (basic - works for 24+ OSes)
- [x] Storage disk creation (qcow2, raw, vdi)
- [x] Architecture detection (PowerPC, Intel, ARM)
- [x] Floppy image creation
- [x] Era-appropriate configuration recommendations
- [x] Test suite (25 tests)
- [x] Documentation
- [x] Website (cleanvm.net)
- [x] GitHub repo
- [x] DNS configuration

## 🚧 IN PROGRESS
- [ ] ISO Detection refinements
  - [ ] Fix "osx leopard install.iso" detection (server keyword conflict)
  - [ ] DMG file support for modern macOS
  - [ ] UDF ISO deep inspection (requires 7z)
  - [ ] Binary inspection for architecture

- [ ] Network Manager
  - [ ] Actual NAT implementation (started, not complete)
  - [ ] Bridge mode implementation (started, not complete)
  - [ ] Network validation
  - [ ] IP assignment

- [ ] Storage Manager
  - [ ] Disk resizing
  - [ ] Snapshot creation/management
  - [ ] Disk conversion between formats
  - [ ] Storage pool management

## ❌ NOT STARTED (but docs promise it!)

### Core VM Management
- [ ] VirtualMachineManager.CreateAsync() - actually create VMs with libvirt
- [ ] VirtualMachineManager.StartAsync() - actually start VMs
- [ ] VirtualMachineManager.StopAsync() - actually stop VMs
- [ ] VirtualMachineManager.DeleteAsync() - remove VMs
- [ ] VirtualMachineManager.ListAsync() - list all VMs
- [ ] VM state tracking (running, stopped, paused)
- [ ] Console access to VMs

### Hypervisor Backend
- [ ] LibvirtHypervisorBackend - complete implementation
- [ ] libvirt XML generation for VMs
- [ ] Connection management
- [ ] Error handling
- [ ] Resource validation

### Enterprise Features
- [ ] License validation (currently just stub)
- [ ] License generation
- [ ] Feature gating (Community vs Enterprise)
- [ ] License storage/retrieval
- [ ] Online activation (optional)

### CLI
- [ ] `cleanvm create` command
- [ ] `cleanvm start` command
- [ ] `cleanvm stop` command
- [ ] `cleanvm list` command
- [ ] `cleanvm delete` command
- [ ] `cleanvm detect` command (for ISOs)
- [ ] `cleanvm console` command
- [ ] Interactive prompts
- [ ] Progress indicators

### Web UI (Blazor)
- [ ] Dashboard showing VMs
- [ ] Create VM wizard
- [ ] VM control (start/stop/delete)
- [ ] VM console viewer
- [ ] ISO library browser
- [ ] Storage management UI
- [ ] Network configuration UI
- [ ] Settings page

### Desktop UI (Avalonia)
- [ ] Main window with VM list
- [ ] Create VM dialog
- [ ] VM control buttons
- [ ] Settings window
- [ ] Console viewer
- [ ] Cross-platform support (Windows/Linux/macOS)

### Database
- [ ] VM metadata storage
- [ ] Configuration persistence
- [ ] ISO library database
- [ ] License storage
- [ ] Choose database (SQLite? PostgreSQL?)

### ISO Library Manager
- [ ] Scan directories (implemented but not tested)
- [ ] Search functionality (implemented but not tested)
- [ ] Caching system
- [ ] Automatic downloads from official sources
- [ ] ISO verification (checksums)

### Advanced Features
- [ ] VM snapshots
- [ ] VM cloning
- [ ] VM templates
- [ ] Shared folders
- [ ] USB passthrough
- [ ] PCI passthrough
- [ ] Live migration
- [ ] Clustering support (Enterprise)

### Documentation
- [ ] CLI usage guide
- [ ] Web UI user guide
- [ ] API reference
- [ ] Troubleshooting guide
- [ ] Migration guides (from Proxmox, ESXi, etc.)

### Testing
- [ ] Integration tests for VM lifecycle
- [ ] Network tests
- [ ] Storage tests
- [ ] UI tests
- [ ] Performance benchmarks

### Security
- [ ] User authentication
- [ ] Permission system
- [ ] Audit logging
- [ ] Secure VM isolation
- [ ] API authentication

### DevOps
- [ ] CI/CD pipeline
- [ ] Automated releases
- [ ] Package for different distros (deb, rpm, etc.)
- [ ] Docker image
- [ ] Installation scripts

## Priority Order

### Phase 1 - Make VMs Actually Work
1. Complete LibvirtHypervisorBackend
2. Implement VirtualMachineManager CRUD operations
3. Fix Network Manager (NAT at minimum)
4. Working CLI commands (create, start, stop, list)
5. Database for VM tracking

### Phase 2 - User Interface
1. Functional CLI with all commands
2. Basic Web UI (dashboard + create VM)
3. Desktop app basic functionality

### Phase 3 - Enterprise & Advanced
1. License system
2. Snapshots
3. Clustering
4. Advanced features

### Phase 4 - Polish
1. Complete documentation
2. Migration tools
3. Packaging
4. Community building

---

**Reality Check**: We have ~20% of a working product. Great foundation, great vision, lots of work ahead!

💙🏴‍☠️ Captain CP & Daniel - December 2024
