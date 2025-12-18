using CleanVM.Core.Interfaces;
using CleanVM.Core.Models;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace CleanVM.Core.Hypervisor;

/// <summary>
/// QEMU/KVM hypervisor backend using libvirt
/// </summary>
public class LibvirtHypervisorBackend : IHypervisorBackend
{
    private readonly string _connectionUri;
    private readonly string _storagePoolPath;

    public LibvirtHypervisorBackend(string connectionUri = "qemu:///system", string storagePoolPath = "/var/lib/libvirt/images")
    {
        _connectionUri = connectionUri;
        _storagePoolPath = storagePoolPath;
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var result = await RunVirshCommandAsync("version");
            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> CreateVMAsync(Interfaces.VMConfiguration config, string name)
    {
        // Generate unique VM ID
        var vmId = Guid.NewGuid().ToString();
        
        // Create disk image
        var diskPath = Path.Combine(_storagePoolPath, $"{vmId}.qcow2");
        await CreateDiskImageAsync(diskPath, config.DiskSizeGB);
        
        // Generate libvirt XML domain definition
        var domainXml = GenerateDomainXml(vmId, name, config, diskPath);
        
        // Define the VM in libvirt
        var xmlFile = Path.Combine(Path.GetTempPath(), $"{vmId}.xml");
        await File.WriteAllTextAsync(xmlFile, domainXml);
        
        try
        {
            await RunVirshCommandAsync($"define {xmlFile}");
            return vmId;
        }
        finally
        {
            File.Delete(xmlFile);
        }
    }

    public async Task StartVMAsync(string vmId)
    {
        await RunVirshCommandAsync($"start {vmId}");
    }

    public async Task StopVMAsync(string vmId, bool force)
    {
        var command = force ? $"destroy {vmId}" : $"shutdown {vmId}";
        await RunVirshCommandAsync(command);
    }

    public async Task PauseVMAsync(string vmId)
    {
        await RunVirshCommandAsync($"suspend {vmId}");
    }

    public async Task ResumeVMAsync(string vmId)
    {
        await RunVirshCommandAsync($"resume {vmId}");
    }

    public async Task DeleteVMAsync(string vmId)
    {
        // Undefine the VM
        await RunVirshCommandAsync($"undefine {vmId} --remove-all-storage");
    }

    public async Task<VMState> GetStateAsync(string vmId)
    {
        var result = await RunVirshCommandAsync($"domstate {vmId}");
        if (result.ExitCode != 0)
            return VMState.Error;

        var state = result.Output.Trim().ToLowerInvariant();
        return state switch
        {
            "running" => VMState.Running,
            "paused" => VMState.Paused,
            "shut off" => VMState.Stopped,
            "shutoff" => VMState.Stopped,
            _ => VMState.Error
        };
    }

    public async Task<string> GetVNCAddressAsync(string vmId)
    {
        var result = await RunVirshCommandAsync($"vncdisplay {vmId}");
        if (result.ExitCode == 0)
            return result.Output.Trim();
        
        return string.Empty;
    }

    private async Task CreateDiskImageAsync(string diskPath, long sizeGB)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "qemu-img",
                Arguments = $"create -f qcow2 {diskPath} {sizeGB}G",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Failed to create disk image: {error}");
        }
    }

    private string GenerateDomainXml(string vmId, string name, Interfaces.VMConfiguration config, string diskPath)
    {
        var xml = new XElement("domain",
            new XAttribute("type", "kvm"),
            new XElement("name", vmId),
            new XElement("uuid", vmId),
            new XElement("memory",
                new XAttribute("unit", "MiB"),
                config.MemoryMB),
            new XElement("vcpu", config.CpuCores),
            new XElement("os",
                new XElement("type",
                    new XAttribute("arch", "x86_64"),
                    new XAttribute("machine", "pc"),
                    "hvm"),
                new XElement("boot",
                    new XAttribute("dev", config.IsoPath != null ? "cdrom" : "hd"))
            ),
            new XElement("features",
                new XElement("acpi"),
                new XElement("apic")
            ),
            new XElement("cpu",
                new XAttribute("mode", "host-passthrough")
            ),
            new XElement("clock",
                new XAttribute("offset", "utc")
            ),
            new XElement("on_poweroff", "destroy"),
            new XElement("on_reboot", "restart"),
            new XElement("on_crash", "destroy"),
            new XElement("devices",
                // Disk
                new XElement("disk",
                    new XAttribute("type", "file"),
                    new XAttribute("device", "disk"),
                    new XElement("driver",
                        new XAttribute("name", "qemu"),
                        new XAttribute("type", "qcow2")
                    ),
                    new XElement("source",
                        new XAttribute("file", diskPath)
                    ),
                    new XElement("target",
                        new XAttribute("dev", "vda"),
                        new XAttribute("bus", "virtio")
                    )
                ),
                // Network
                new XElement("interface",
                    new XAttribute("type", config.NetworkMode == Interfaces.NetworkMode.NAT ? "network" : "bridge"),
                    new XElement("source",
                        config.NetworkMode == Interfaces.NetworkMode.NAT
                            ? new XAttribute("network", "default")
                            : new XAttribute("bridge", "br0")
                    ),
                    new XElement("model",
                        new XAttribute("type", "virtio")
                    )
                ),
                // Graphics (VNC)
                new XElement("graphics",
                    new XAttribute("type", "vnc"),
                    new XAttribute("port", "-1"),
                    new XAttribute("autoport", "yes"),
                    new XAttribute("listen", "0.0.0.0")
                ),
                // Console
                new XElement("console",
                    new XAttribute("type", "pty")
                )
            )
        );

        // Add ISO if specified
        if (!string.IsNullOrEmpty(config.IsoPath))
        {
            var devices = xml.Element("devices");
            devices?.Add(
                new XElement("disk",
                    new XAttribute("type", "file"),
                    new XAttribute("device", "cdrom"),
                    new XElement("driver",
                        new XAttribute("name", "qemu"),
                        new XAttribute("type", "raw")
                    ),
                    new XElement("source",
                        new XAttribute("file", config.IsoPath)
                    ),
                    new XElement("target",
                        new XAttribute("dev", "hdc"),
                        new XAttribute("bus", "ide")
                    ),
                    new XElement("readonly")
                )
            );
        }

        // Add floppy if specified (for older Windows that need driver floppies)
        if (!string.IsNullOrEmpty(config.FloppyPath))
        {
            var devices = xml.Element("devices");
            devices?.Add(
                new XElement("disk",
                    new XAttribute("type", "file"),
                    new XAttribute("device", "floppy"),
                    new XElement("driver",
                        new XAttribute("name", "qemu"),
                        new XAttribute("type", "raw")
                    ),
                    new XElement("source",
                        new XAttribute("file", config.FloppyPath)
                    ),
                    new XElement("target",
                        new XAttribute("dev", "fda"),
                        new XAttribute("bus", "fdc")
                    )
                )
            );
        }

        return xml.ToString();
    }

    private async Task<(int ExitCode, string Output)> RunVirshCommandAsync(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "virsh",
                Arguments = $"-c {_connectionUri} {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        
        var output = new StringBuilder();
        var error = new StringBuilder();
        
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        output.Append(await outputTask);
        error.Append(await errorTask);

        var result = process.ExitCode == 0 ? output.ToString() : error.ToString();
        return (process.ExitCode, result);
    }
}
