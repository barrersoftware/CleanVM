using CleanVM.Core.Services;
using CleanVM.Core.Interfaces;
using CleanVM.Core.Models;

namespace CleanVM.Core.Tests.Services;

public class VirtualMachineManagerTests
{
    private class MockHypervisor : IHypervisorBackend
    {
        public Dictionary<string, string> CreatedVMs = new();
        public HashSet<string> RunningVMs = new();

        public Task<bool> IsAvailableAsync() => Task.FromResult(true);

        public Task<string> CreateVMAsync(Interfaces.VMConfiguration config, string name)
        {
            CreatedVMs[name] = $"vm-{Guid.NewGuid()}";
            return Task.FromResult(CreatedVMs[name]);
        }

        public Task StartVMAsync(string vmId)
        {
            RunningVMs.Add(vmId);
            return Task.CompletedTask;
        }

        public Task StopVMAsync(string vmId, bool force)
        {
            RunningVMs.Remove(vmId);
            return Task.CompletedTask;
        }

        public Task PauseVMAsync(string vmId) => Task.CompletedTask;
        public Task ResumeVMAsync(string vmId) => Task.CompletedTask;
        public Task DeleteVMAsync(string vmId) => Task.CompletedTask;
        public Task<VMState> GetStateAsync(string vmId) => Task.FromResult(RunningVMs.Contains(vmId) ? VMState.Running : VMState.Stopped);
        public Task<string> GetVNCAddressAsync(string vmId) => Task.FromResult("localhost:5900");
    }

    [Fact]
    public async Task CreateVM_ShouldCreateAndTrackVM()
    {
        var mockHypervisor = new MockHypervisor();
        var vmManager = new VirtualMachineManager(mockHypervisor);
        
        var config = new Interfaces.VMConfiguration("test-vm", 2, 2048, 20, null, Interfaces.NetworkMode.NAT);
        var vm = await vmManager.CreateAsync(config, "test-vm");

        Assert.NotNull(vm);
        Assert.Equal("test-vm", vm.Name);
        Assert.Equal(2, vm.Configuration.CpuCores);
        Assert.Equal(2048, vm.Configuration.MemoryMB);
        Assert.Equal(VMState.Stopped, vm.State);
    }

    [Fact]
    public async Task StartVM_ShouldChangeStateToRunning()
    {
        var mockHypervisor = new MockHypervisor();
        var vmManager = new VirtualMachineManager(mockHypervisor);
        
        var config = new Interfaces.VMConfiguration("start-test", 1, 1024, 10, null, Interfaces.NetworkMode.NAT);
        var vm = await vmManager.CreateAsync(config, "start-test");
        await vmManager.StartAsync(vm.Id);

        var state = await vmManager.GetStateAsync(vm.Id);
        Assert.Equal(VMState.Running, state);
    }

    [Fact]
    public async Task StopVM_ShouldChangeStateToStopped()
    {
        var mockHypervisor = new MockHypervisor();
        var vmManager = new VirtualMachineManager(mockHypervisor);
        
        var config = new Interfaces.VMConfiguration("stop-test", 1, 1024, 10, null, Interfaces.NetworkMode.NAT);
        var vm = await vmManager.CreateAsync(config, "stop-test");
        await vmManager.StartAsync(vm.Id);
        await vmManager.StopAsync(vm.Id);

        var state = await vmManager.GetStateAsync(vm.Id);
        Assert.Equal(VMState.Stopped, state);
    }

    [Fact]
    public async Task ListVMs_ShouldReturnAllVMs()
    {
        var mockHypervisor = new MockHypervisor();
        var vmManager = new VirtualMachineManager(mockHypervisor);
        
        var config1 = new Interfaces.VMConfiguration("vm1", 1, 1024, 10, null, Interfaces.NetworkMode.NAT);
        var config2 = new Interfaces.VMConfiguration("vm2", 1, 1024, 10, null, Interfaces.NetworkMode.NAT);

        var vm1 = await vmManager.CreateAsync(config1, "vm1");
        var vm2 = await vmManager.CreateAsync(config2, "vm2");

        var vms = await vmManager.ListAsync();
        Assert.Equal(2, vms.Count());
        Assert.Contains(vms, v => v.Id == vm1.Id);
        Assert.Contains(vms, v => v.Id == vm2.Id);
    }

    [Fact]
    public async Task DeleteVM_ShouldRemoveVM()
    {
        var mockHypervisor = new MockHypervisor();
        var vmManager = new VirtualMachineManager(mockHypervisor);
        
        var config = new Interfaces.VMConfiguration("delete-test", 1, 1024, 10, null, Interfaces.NetworkMode.NAT);
        var vm = await vmManager.CreateAsync(config, "delete-test");
        await vmManager.DeleteAsync(vm.Id);

        var vms = await vmManager.ListAsync();
        Assert.DoesNotContain(vms, v => v.Id == vm.Id);
    }

    [Fact]
    public async Task PauseAndResumeVM_ShouldChangeStates()
    {
        var mockHypervisor = new MockHypervisor();
        var vmManager = new VirtualMachineManager(mockHypervisor);
        
        var config = new Interfaces.VMConfiguration("pause-test", 1, 1024, 10, null, Interfaces.NetworkMode.NAT);
        var vm = await vmManager.CreateAsync(config, "pause-test");
        await vmManager.StartAsync(vm.Id);
        
        await vmManager.PauseAsync(vm.Id);
        var pausedState = await vmManager.GetStateAsync(vm.Id);

        await vmManager.ResumeAsync(vm.Id);
        var resumedState = await vmManager.GetStateAsync(vm.Id);

        Assert.Equal(VMState.Paused, pausedState);
        Assert.Equal(VMState.Running, resumedState);
    }
}
