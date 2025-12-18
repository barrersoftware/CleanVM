# CleanVM - Project Architecture

**Tech Stack:** .NET 9 / C#  
**Pattern:** Clean Architecture with shared core  
**Created:** December 16, 2025

---

## Solution Structure

```
CleanVM.sln                          # Root solution file
│
├── src/                             # Source code
│   │
│   ├── CleanVM.Core/                # Shared business logic (Class Library)
│   │   ├── CleanVM.Core.csproj
│   │   ├── VM/                      # Virtual machine management
│   │   │   ├── IVmManager.cs
│   │   │   ├── VmManager.cs
│   │   │   ├── VmConfig.cs
│   │   │   ├── VmState.cs
│   │   │   └── VmLifecycle.cs
│   │   │
│   │   ├── Storage/                 # Disk and storage
│   │   │   ├── IStorageManager.cs
│   │   │   ├── StorageManager.cs
│   │   │   ├── DiskManager.cs
│   │   │   ├── SnapshotManager.cs
│   │   │   └── DiskFormats/
│   │   │       ├── VdiFormat.cs
│   │   │       ├── VmdkFormat.cs
│   │   │       └── RawFormat.cs
│   │   │
│   │   ├── Network/                 # Networking
│   │   │   ├── INetworkManager.cs
│   │   │   ├── NetworkManager.cs
│   │   │   ├── BridgeNetwork.cs
│   │   │   ├── NatNetwork.cs
│   │   │   └── NetworkAdapter.cs
│   │   │
│   │   ├── ISO/                     # ISO management
│   │   │   ├── IIsoManager.cs
│   │   │   ├── IsoManager.cs
│   │   │   ├── IsoLibrary.cs
│   │   │   ├── IsoDetector.cs
│   │   │   └── IsoDownloader.cs
│   │   │
│   │   ├── Hypervisor/              # Low-level virtualization
│   │   │   ├── IHypervisor.cs
│   │   │   ├── LibvirtHypervisor.cs
│   │   │   ├── QemuInterface.cs
│   │   │   └── HardwareEmulation/
│   │   │       ├── CpuEmulation.cs
│   │   │       ├── MemoryEmulation.cs
│   │   │       └── DeviceEmulation.cs
│   │   │
│   │   ├── License/                 # License validation
│   │   │   ├── ILicenseValidator.cs
│   │   │   ├── LicenseValidator.cs
│   │   │   ├── LicenseKey.cs
│   │   │   └── Features.cs
│   │   │
│   │   ├── Models/                  # Shared data models
│   │   │   ├── Vm.cs
│   │   │   ├── Disk.cs
│   │   │   ├── Network.cs
│   │   │   ├── Snapshot.cs
│   │   │   └── IsoMetadata.cs
│   │   │
│   │   ├── Interfaces/              # Core interfaces
│   │   │   ├── IVmManager.cs
│   │   │   ├── IStorageManager.cs
│   │   │   ├── INetworkManager.cs
│   │   │   └── IHypervisor.cs
│   │   │
│   │   └── Common/                  # Utilities
│   │       ├── Logger.cs
│   │       ├── Config.cs
│   │       ├── Constants.cs
│   │       └── Extensions.cs
│   │
│   ├── CleanVM.Enterprise/          # Enterprise features (CLOSED SOURCE)
│   │   ├── CleanVM.Enterprise.csproj
│   │   ├── SSO/                     # Single sign-on
│   │   │   ├── ISsoProvider.cs
│   │   │   ├── LdapProvider.cs
│   │   │   ├── SamlProvider.cs
│   │   │   └── OAuthProvider.cs
│   │   │
│   │   ├── RBAC/                    # Role-based access control
│   │   │   ├── IRbacManager.cs
│   │   │   ├── RbacManager.cs
│   │   │   ├── Role.cs
│   │   │   ├── Permission.cs
│   │   │   └── Department.cs
│   │   │
│   │   ├── HA/                      # High availability
│   │   │   ├── IHaCluster.cs
│   │   │   ├── ClusterManager.cs
│   │   │   ├── LiveMigration.cs
│   │   │   └── Failover.cs
│   │   │
│   │   ├── Quota/                   # Resource quotas
│   │   │   ├── IQuotaManager.cs
│   │   │   ├── QuotaManager.cs
│   │   │   ├── UserQuota.cs
│   │   │   └── TeamQuota.cs
│   │   │
│   │   └── Compliance/              # Reporting & compliance
│   │       ├── IComplianceReporter.cs
│   │       ├── ComplianceReporter.cs
│   │       ├── AuditLog.cs
│   │       └── Reports.cs
│   │
│   ├── CleanVM.Web/                 # Web UI + API (ASP.NET Core)
│   │   ├── CleanVM.Web.csproj
│   │   ├── Program.cs               # Entry point
│   │   ├── Startup.cs               # Configuration
│   │   │
│   │   ├── Controllers/             # REST API
│   │   │   ├── VmController.cs
│   │   │   ├── StorageController.cs
│   │   │   ├── NetworkController.cs
│   │   │   ├── IsoController.cs
│   │   │   └── LicenseController.cs
│   │   │
│   │   ├── Pages/                   # Blazor pages
│   │   │   ├── Index.razor          # Dashboard
│   │   │   ├── VmList.razor         # VM list
│   │   │   ├── VmDetails.razor      # VM details
│   │   │   ├── VmCreate.razor       # Create wizard
│   │   │   ├── Storage.razor        # Storage management
│   │   │   ├── Network.razor        # Network config
│   │   │   ├── IsoLibrary.razor     # ISO browser
│   │   │   └── Settings.razor       # Settings
│   │   │
│   │   ├── Components/              # Reusable Blazor components
│   │   │   ├── VmCard.razor
│   │   │   ├── ResourceChart.razor
│   │   │   ├── ConsoleViewer.razor
│   │   │   └── ProgressIndicator.razor
│   │   │
│   │   ├── Services/                # Frontend services
│   │   │   ├── VmService.cs
│   │   │   ├── StorageService.cs
│   │   │   └── ApiClient.cs
│   │   │
│   │   ├── Hubs/                    # SignalR hubs (real-time)
│   │   │   ├── VmHub.cs
│   │   │   └── ConsoleHub.cs
│   │   │
│   │   ├── wwwroot/                 # Static files
│   │   │   ├── css/
│   │   │   ├── js/
│   │   │   └── images/
│   │   │
│   │   └── appsettings.json         # Configuration
│   │
│   ├── CleanVM.Desktop/             # Desktop UI (Avalonia)
│   │   ├── CleanVM.Desktop.csproj
│   │   ├── Program.cs               # Entry point
│   │   ├── App.axaml                # Application
│   │   │
│   │   ├── Views/                   # UI views
│   │   │   ├── MainWindow.axaml
│   │   │   ├── VmListView.axaml
│   │   │   ├── VmDetailsView.axaml
│   │   │   ├── CreateVmView.axaml
│   │   │   └── SettingsView.axaml
│   │   │
│   │   ├── ViewModels/              # MVVM view models
│   │   │   ├── MainViewModel.cs
│   │   │   ├── VmListViewModel.cs
│   │   │   ├── VmDetailsViewModel.cs
│   │   │   └── CreateVmViewModel.cs
│   │   │
│   │   ├── Services/                # Desktop services
│   │   │   ├── VmService.cs
│   │   │   └── ApiClient.cs
│   │   │
│   │   └── Assets/                  # Icons, images
│   │
│   └── CleanVM.CLI/                 # Command-line interface
│       ├── CleanVM.CLI.csproj
│       ├── Program.cs               # Entry point
│       │
│       ├── Commands/                # CLI commands
│       │   ├── VmCommands.cs        # vm create/start/stop
│       │   ├── StorageCommands.cs   # disk management
│       │   ├── NetworkCommands.cs   # network config
│       │   ├── IsoCommands.cs       # iso operations
│       │   └── ConfigCommands.cs    # configuration
│       │
│       ├── Helpers/                 # CLI utilities
│       │   ├── ConsoleHelper.cs
│       │   ├── OutputFormatter.cs
│       │   └── ProgressBar.cs
│       │
│       └── Options/                 # Command options
│           ├── VmCreateOptions.cs
│           ├── VmStartOptions.cs
│           └── GlobalOptions.cs
│
├── tests/                           # Unit tests
│   ├── CleanVM.Core.Tests/
│   │   ├── VM/
│   │   ├── Storage/
│   │   ├── Network/
│   │   └── ISO/
│   │
│   ├── CleanVM.Web.Tests/
│   │   ├── Controllers/
│   │   └── Pages/
│   │
│   └── CleanVM.CLI.Tests/
│       └── Commands/
│
├── docs/                            # Documentation
│   ├── api/                         # API documentation
│   ├── guides/                      # User guides
│   ├── architecture/                # Technical docs
│   └── examples/                    # Code examples
│
├── scripts/                         # Build/deployment scripts
│   ├── build.sh
│   ├── publish.sh
│   ├── install.sh
│   └── package.sh
│
├── tools/                           # Development tools
│   └── license-generator/          # Enterprise key generator
│
├── .github/                         # GitHub config
│   ├── workflows/                   # CI/CD
│   │   ├── build.yml
│   │   └── test.yml
│   └── ISSUE_TEMPLATE/
│
├── README.md                        # Project overview
├── LICENSE                          # MIT License
├── CONTRIBUTING.md                  # Contribution guide
├── CHANGELOG.md                     # Version history
└── .gitignore                       # Git ignore rules
```

---

## Project Dependencies

```
CleanVM.Core (Class Library)
  ↓ No dependencies on other CleanVM projects
  ↓ Pure business logic
  
CleanVM.Enterprise (Class Library)
  ↓ References: CleanVM.Core
  ↓ Extends core with enterprise features
  
CleanVM.Web (ASP.NET Core)
  ↓ References: CleanVM.Core, CleanVM.Enterprise
  ↓ Web UI + REST API
  
CleanVM.Desktop (Avalonia)
  ↓ References: CleanVM.Core, CleanVM.Enterprise
  ↓ Native desktop app
  
CleanVM.CLI (Console App)
  ↓ References: CleanVM.Core, CleanVM.Enterprise
  ↓ Command-line interface
```

---

## Key Design Decisions

### 1. Shared Core Logic

**All UIs use the same `CleanVM.Core` library:**
- Write VM management logic once
- Web, Desktop, and CLI all call same methods
- Consistent behavior everywhere
- Easy to test

**Example:**
```csharp
// CleanVM.Core/VM/VmManager.cs
public class VmManager : IVmManager
{
    public async Task<Vm> CreateVmAsync(VmConfig config)
    {
        // Business logic here
        // Called by Web, Desktop, and CLI
    }
}

// CleanVM.Web/Controllers/VmController.cs
[HttpPost("api/vms")]
public async Task<IActionResult> CreateVm([FromBody] VmConfig config)
{
    var vm = await _vmManager.CreateVmAsync(config);
    return Ok(vm);
}

// CleanVM.Desktop/ViewModels/CreateVmViewModel.cs
public async Task CreateVm()
{
    var vm = await _vmManager.CreateVmAsync(Config);
    // Update UI
}

// CleanVM.CLI/Commands/VmCommands.cs
[Command("vm create")]
public async Task CreateVm(VmCreateOptions options)
{
    var vm = await _vmManager.CreateVmAsync(options.ToConfig());
    Console.WriteLine($"Created VM: {vm.Name}");
}
```

### 2. Enterprise Features Separation

**CleanVM.Enterprise is a separate project:**
- Community: Just CleanVM.Core
- Enterprise: CleanVM.Core + CleanVM.Enterprise
- Same binary, feature unlock via license
- Enterprise code stays closed-source

**Build Process:**
```bash
# Community build (open source)
dotnet build src/CleanVM.Core
dotnet build src/CleanVM.Web --exclude-enterprise

# Enterprise build (includes enterprise)
dotnet build src/CleanVM.Core
dotnet build src/CleanVM.Enterprise
dotnet build src/CleanVM.Web --include-enterprise
```

### 3. Multiple Interfaces, One Engine

**Architecture Pattern:**
```
┌─────────────────────────────────────────┐
│           CleanVM.Core                  │
│      (Shared Business Logic)            │
│                                         │
│  • VM Management                        │
│  • Storage Management                   │
│  • Network Management                   │
│  • ISO Management                       │
│  • Hypervisor Interface                 │
└──────────────┬──────────────────────────┘
               │
        ┌──────┴──────┬──────────┐
        │             │          │
┌───────▼──────┐ ┌────▼─────┐ ┌─▼────────┐
│   Web UI     │ │ Desktop  │ │   CLI    │
│   (Blazor)   │ │(Avalonia)│ │(Console) │
│              │ │          │ │          │
│ REST API     │ │  Native  │ │Commands  │
│ Web Console  │ │  UI      │ │Scripts   │
└──────────────┘ └──────────┘ └──────────┘
```

### 4. Adaptive Installation

**Single installer, smart detection:**
```csharp
// CleanVM.Installer/Program.cs
public static void Main(string[] args)
{
    var environment = DetectEnvironment();
    
    if (environment == Environment.Desktop)
    {
        InstallWebUI();
        InstallDesktopUI();
        Console.WriteLine("Installed: Web + Desktop UI");
        Console.WriteLine("Run: cleanvm (desktop) or cleanvm --web");
    }
    else if (environment == Environment.Server)
    {
        InstallWebUI();
        InstallSystemdService();
        Console.WriteLine("Installed: Web UI (headless)");
        Console.WriteLine("Access: http://server:8080");
    }
    
    // Manual override
    if (args.Contains("--web-only"))
    {
        InstallWebUI();
    }
}
```

---

## Technology Choices

### Core (.NET 9)
- **Language:** C# 13
- **Framework:** .NET 9
- **Platform:** Cross-platform (Linux, Windows, macOS)

### Web (ASP.NET Core + Blazor)
- **Framework:** ASP.NET Core 9
- **UI:** Blazor Server or WebAssembly
- **API:** REST + SignalR
- **Auth:** JWT + Cookie
- **Real-time:** SignalR for live updates

### Desktop (Avalonia)
- **Framework:** Avalonia 11+
- **Pattern:** MVVM
- **Platform:** Windows, Linux, macOS
- **Rendering:** Skia (hardware-accelerated)

### CLI (System.CommandLine)
- **Library:** System.CommandLine
- **Output:** Text, JSON, Table formats
- **Interactive:** Progress bars, prompts

### Database (SQLite)
- **Storage:** SQLite (embedded)
- **ORM:** Entity Framework Core
- **Data:** VM configs, snapshots, metadata
- **Backup:** Simple file copy

### Virtualization (Libvirt/QEMU)
- **Library:** Libvirt C# bindings
- **P/Invoke:** Direct QEMU calls when needed
- **Platforms:** KVM (Linux), Hyper-V (Windows), HVF (macOS)

---

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Build specific project
dotnet build src/CleanVM.Core
dotnet build src/CleanVM.Web
dotnet build src/CleanVM.Desktop
dotnet build src/CleanVM.CLI

# Run tests
dotnet test

# Publish self-contained
dotnet publish src/CleanVM.Web -c Release -r linux-x64 --self-contained
dotnet publish src/CleanVM.Desktop -c Release -r linux-x64 --self-contained
dotnet publish src/CleanVM.CLI -c Release -r linux-x64 --self-contained

# Run locally
dotnet run --project src/CleanVM.Web
dotnet run --project src/CleanVM.Desktop
dotnet run --project src/CleanVM.CLI -- vm list
```

---

## File Naming Conventions

**C# Code:**
- Classes: `PascalCase.cs` (e.g., `VmManager.cs`)
- Interfaces: `IPascalCase.cs` (e.g., `IVmManager.cs`)
- Enums: `PascalCase.cs` (e.g., `VmState.cs`)

**Blazor:**
- Pages: `PascalCase.razor` (e.g., `VmList.razor`)
- Components: `PascalCase.razor` (e.g., `VmCard.razor`)

**Avalonia:**
- Views: `PascalCaseView.axaml` (e.g., `VmListView.axaml`)
- ViewModels: `PascalCaseViewModel.cs` (e.g., `VmListViewModel.cs`)

**CLI:**
- Commands: `PascalCaseCommands.cs` (e.g., `VmCommands.cs`)
- Options: `PascalCaseOptions.cs` (e.g., `VmCreateOptions.cs`)

---

## Next Steps

1. ✅ **Project structure defined** (this document)
2. ⬜ Create .NET solution and projects
3. ⬜ Set up basic project structure
4. ⬜ Implement CleanVM.Core interfaces
5. ⬜ Build proof-of-concept VM boot
6. ⬜ Develop CLI first (simplest)
7. ⬜ Add Web UI
8. ⬜ Add Desktop UI
9. ⬜ Implement Enterprise features
10. ⬜ Package and release

---

**Created:** December 16, 2025  
**Status:** Design Complete, Ready for Implementation  
**By:** Captain CP & Daniel Elliott

🏴‍☠️ **Clean architecture. Clear boundaries. Ready to build.**
