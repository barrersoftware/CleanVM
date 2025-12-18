using CleanVM.Core.Interfaces;
using CleanVM.Core.Models;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace CleanVM.Core.Network;

/// <summary>
/// Manages virtual networks, network interfaces, and network operations
/// </summary>
public class NetworkManager : INetworkManager
{
    private readonly Dictionary<string, Models.Network> _networks = new();
    private readonly Dictionary<string, NetworkInterface> _interfaces = new();
    private readonly Dictionary<string, DhcpLease> _dhcpLeases = new();
    private readonly string _connectionUri;

    public NetworkManager(string connectionUri = "qemu:///system")
    {
        _connectionUri = connectionUri;
    }

    public async Task<Models.Network> CreateNetworkAsync(NetworkCreateRequest request, CancellationToken cancellationToken = default)
    {
        var networkId = Guid.NewGuid().ToString();
        
        var network = new Models.Network
        {
            Id = networkId,
            Name = request.Name,
            Type = request.Type,
            BridgeName = request.BridgeName,
            Subnet = request.Subnet ?? "192.168.100.0/24",
            EnableDhcp = request.EnableDhcp,
            CreatedAt = DateTime.UtcNow
        };

        // Create network in libvirt
        if (request.Type == NetworkType.NAT || request.Type == NetworkType.Internal)
        {
            var networkXml = GenerateNetworkXml(network);
            await DefineNetworkAsync(networkId, networkXml, cancellationToken);
            await StartNetworkAsync(networkId, cancellationToken);
        }
        
        _networks[networkId] = network;
        return network;
    }

    public Task<Models.Network> GetNetworkAsync(string networkId, CancellationToken cancellationToken = default)
    {
        if (!_networks.TryGetValue(networkId, out var network))
            throw new KeyNotFoundException($"Network {networkId} not found");
        return Task.FromResult(network);
    }

    public Task<IEnumerable<Models.Network>> ListNetworksAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Models.Network>>(_networks.Values);
    }

    public async Task DeleteNetworkAsync(string networkId, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkAsync(networkId, cancellationToken);
        
        // Check if any interfaces are using this network
        var connectedInterfaces = _interfaces.Values.Where(i => i.NetworkId == networkId);
        if (connectedInterfaces.Any())
            throw new InvalidOperationException($"Network has {connectedInterfaces.Count()} connected interfaces");
        
        // Stop and undefine network in libvirt
        await StopNetworkAsync(networkId, cancellationToken);
        await UndefineNetworkAsync(networkId, cancellationToken);
        
        _networks.Remove(networkId);
    }

    public async Task<NetworkInterface> CreateInterfaceAsync(string vmId, NetworkInterfaceCreateRequest request, CancellationToken cancellationToken = default)
    {
        var interfaceId = Guid.NewGuid().ToString();
        
        var networkInterface = new NetworkInterface
        {
            Id = interfaceId,
            VmId = vmId,
            NetworkId = request.NetworkId,
            MacAddress = request.MacAddress ?? GenerateMacAddress(),
            Model = request.Model
        };
        
        _interfaces[interfaceId] = networkInterface;
        return networkInterface;
    }

    public Task<IEnumerable<NetworkInterface>> GetVmInterfacesAsync(string vmId, CancellationToken cancellationToken = default)
    {
        var vmInterfaces = _interfaces.Values.Where(i => i.VmId == vmId);
        return Task.FromResult(vmInterfaces);
    }

    public async Task DeleteInterfaceAsync(string vmId, string interfaceId, CancellationToken cancellationToken = default)
    {
        if (!_interfaces.TryGetValue(interfaceId, out var iface))
            throw new KeyNotFoundException($"Interface {interfaceId} not found");
        
        if (iface.VmId != vmId)
            throw new InvalidOperationException($"Interface {interfaceId} does not belong to VM {vmId}");
        
        _interfaces.Remove(interfaceId);
    }

    public async Task AttachInterfaceAsync(string vmId, string networkId, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkAsync(networkId, cancellationToken);
        
        var request = new NetworkInterfaceCreateRequest
        {
            NetworkId = networkId,
            Model = NetworkInterfaceModel.VirtIO
        };
        
        await CreateInterfaceAsync(vmId, request, cancellationToken);
    }

    public async Task DetachInterfaceAsync(string vmId, string interfaceId, CancellationToken cancellationToken = default)
    {
        await DeleteInterfaceAsync(vmId, interfaceId, cancellationToken);
    }

    public async Task<NetworkStats> GetNetworkStatsAsync(string networkId, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkAsync(networkId, cancellationToken);
        var connectedVms = _interfaces.Values.Count(i => i.NetworkId == networkId);
        
        return new NetworkStats
        {
            RxBytes = 0, // Would query from libvirt
            TxBytes = 0,
            RxPackets = 0,
            TxPackets = 0,
            ConnectedVms = connectedVms,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<NetworkInterfaceStats> GetInterfaceStatsAsync(string vmId, string interfaceId, CancellationToken cancellationToken = default)
    {
        if (!_interfaces.TryGetValue(interfaceId, out var iface))
            throw new KeyNotFoundException($"Interface {interfaceId} not found");
        
        if (iface.VmId != vmId)
            throw new InvalidOperationException($"Interface does not belong to VM {vmId}");
        
        return new NetworkInterfaceStats
        {
            RxBytes = 0, // Would query from libvirt/hypervisor
            TxBytes = 0,
            RxPackets = 0,
            TxPackets = 0,
            Timestamp = DateTime.UtcNow
        };
    }

    public Task<DhcpLease> GetDhcpLeaseAsync(string vmId, CancellationToken cancellationToken = default)
    {
        if (!_dhcpLeases.TryGetValue(vmId, out var lease))
            throw new KeyNotFoundException($"No DHCP lease found for VM {vmId}");
        return Task.FromResult(lease);
    }

    public async Task<IEnumerable<DhcpLease>> ListDhcpLeasesAsync(string networkId, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkAsync(networkId, cancellationToken);
        
        // Get interfaces on this network
        var interfacesOnNetwork = _interfaces.Values.Where(i => i.NetworkId == networkId);
        
        // Return leases for VMs on this network
        var leases = _dhcpLeases.Values
            .Where(l => interfacesOnNetwork.Any(i => i.VmId == l.VmId));
        
        return leases;
    }

    private string GenerateNetworkXml(Models.Network network)
    {
        var xml = new XElement("network",
            new XElement("name", network.Id),
            new XElement("uuid", network.Id)
        );

        if (network.Type == NetworkType.NAT)
        {
            xml.Add(new XElement("forward", new XAttribute("mode", "nat")));
            
            // Parse subnet (e.g., "192.168.100.0/24")
            var subnetParts = network.Subnet?.Split('/');
            if (subnetParts?.Length == 2)
            {
                var ipBase = subnetParts[0];
                var ipParts = ipBase.Split('.');
                var gatewayIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.1";
                
                xml.Add(new XElement("ip",
                    new XAttribute("address", gatewayIp),
                    new XAttribute("netmask", "255.255.255.0")
                ));

                if (network.EnableDhcp)
                {
                    var dhcpStart = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.100";
                    var dhcpEnd = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.254";
                    
                    xml.Element("ip")?.Add(
                        new XElement("dhcp",
                            new XElement("range",
                                new XAttribute("start", dhcpStart),
                                new XAttribute("end", dhcpEnd)
                            )
                        )
                    );
                }
            }
        }
        else if (network.Type == NetworkType.Bridge && !string.IsNullOrEmpty(network.BridgeName))
        {
            xml.Add(
                new XElement("forward", new XAttribute("mode", "bridge")),
                new XElement("bridge", new XAttribute("name", network.BridgeName))
            );
        }
        else if (network.Type == NetworkType.Internal)
        {
            // Internal network - no forwarding
        }

        return xml.ToString();
    }

    private async Task DefineNetworkAsync(string networkId, string xml, CancellationToken cancellationToken)
    {
        var xmlFile = Path.Combine(Path.GetTempPath(), $"network-{networkId}.xml");
        await File.WriteAllTextAsync(xmlFile, xml, cancellationToken);
        
        try
        {
            await RunVirshCommandAsync($"net-define {xmlFile}", cancellationToken);
        }
        finally
        {
            File.Delete(xmlFile);
        }
    }

    private async Task StartNetworkAsync(string networkId, CancellationToken cancellationToken)
    {
        await RunVirshCommandAsync($"net-start {networkId}", cancellationToken);
        await RunVirshCommandAsync($"net-autostart {networkId}", cancellationToken);
    }

    private async Task StopNetworkAsync(string networkId, CancellationToken cancellationToken)
    {
        try
        {
            await RunVirshCommandAsync($"net-destroy {networkId}", cancellationToken);
        }
        catch
        {
            // Network might not be running
        }
    }

    private async Task UndefineNetworkAsync(string networkId, CancellationToken cancellationToken)
    {
        await RunVirshCommandAsync($"net-undefine {networkId}", cancellationToken);
    }

    private string GenerateMacAddress()
    {
        var random = new Random();
        var macBytes = new byte[6];
        random.NextBytes(macBytes);
        
        // Set locally administered bit
        macBytes[0] = (byte)(macBytes[0] & 0xFE | 0x02);
        
        return string.Join(":", macBytes.Select(b => b.ToString("x2")));
    }

    private async Task<(int ExitCode, string Output)> RunVirshCommandAsync(string arguments, CancellationToken cancellationToken)
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
        
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        
        await process.WaitForExitAsync(cancellationToken);
        
        output.Append(await outputTask);
        error.Append(await errorTask);

        var result = process.ExitCode == 0 ? output.ToString() : error.ToString();
        return (process.ExitCode, result);
    }
}
