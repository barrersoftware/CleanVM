using CleanVM.Core.Interfaces;
using CleanVM.Core.Models;

namespace CleanVM.Core.Services;

public class VirtualMachineManager : IVirtualMachineManager
{
    private readonly IHypervisorBackend _hypervisor;
    private readonly Dictionary<Guid, VirtualMachine> _vms = new();

    public VirtualMachineManager(IHypervisorBackend hypervisor)
    {
        _hypervisor = hypervisor;
    }

    public async Task<VirtualMachine> CreateAsync(Interfaces.VMConfiguration config, string name)
    {
        var vm = new VirtualMachine
        {
            Id = Guid.NewGuid(),
            Name = name,
            Configuration = new Models.VMConfiguration 
            { 
                CpuCores = config.CpuCores,
                MemoryMB = config.MemoryMB,
                DiskSizeGB = config.DiskSizeGB,
                IsoPath = config.IsoPath,
                NetworkMode = config.NetworkMode == Interfaces.NetworkMode.NAT ? Models.NetworkMode.NAT : Models.NetworkMode.Bridged,
                DetectedOS = config.DetectedOS != null ? ConvertOSType(config.DetectedOS.Type) : Models.OSType.Unknown
            },
            State = VMState.Stopped,
            CreatedAt = DateTime.UtcNow
        };

        await _hypervisor.CreateVMAsync(config, vm.Id.ToString());
        _vms[vm.Id] = vm;

        return vm;
    }

    public Task<VirtualMachine> GetAsync(Guid id)
    {
        if (!_vms.TryGetValue(id, out var vm))
            throw new KeyNotFoundException($"VM {id} not found");
        return Task.FromResult(vm);
    }

    public Task<IEnumerable<VirtualMachine>> ListAsync()
    {
        return Task.FromResult<IEnumerable<VirtualMachine>>(_vms.Values);
    }

    public async Task StartAsync(Guid id)
    {
        var vm = await GetAsync(id);
        vm.State = VMState.Starting;
        await _hypervisor.StartVMAsync(id.ToString());
        vm.State = VMState.Running;
        vm.LastStartedAt = DateTime.UtcNow;
    }

    public async Task StopAsync(Guid id, bool force = false)
    {
        var vm = await GetAsync(id);
        vm.State = VMState.Stopping;
        await _hypervisor.StopVMAsync(id.ToString(), force);
        vm.State = VMState.Stopped;
    }

    public async Task PauseAsync(Guid id)
    {
        var vm = await GetAsync(id);
        await _hypervisor.PauseVMAsync(id.ToString());
        vm.State = VMState.Paused;
    }

    public async Task ResumeAsync(Guid id)
    {
        var vm = await GetAsync(id);
        await _hypervisor.ResumeVMAsync(id.ToString());
        vm.State = VMState.Running;
    }

    public async Task DeleteAsync(Guid id)
    {
        var vm = await GetAsync(id);
        if (vm.State != VMState.Stopped)
            throw new InvalidOperationException("VM must be stopped before deletion");
        
        await _hypervisor.DeleteVMAsync(id.ToString());
        _vms.Remove(id);
    }

    public async Task<VMState> GetStateAsync(Guid id)
    {
        var vm = await GetAsync(id);
        return vm.State;
    }

    private Models.OSType ConvertOSType(Interfaces.OSType type)
    {
        return type switch
        {
            Interfaces.OSType.Windows => Models.OSType.WindowsServer,
            Interfaces.OSType.Linux => Models.OSType.UbuntuServer,
            Interfaces.OSType.MacOS => Models.OSType.MacOS,
            _ => Models.OSType.Unknown
        };
    }
}
