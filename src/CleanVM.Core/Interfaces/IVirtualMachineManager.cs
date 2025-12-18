using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface IVirtualMachineManager
{
    Task<VirtualMachine> CreateAsync(VMConfiguration config, string name);
    Task<VirtualMachine> GetAsync(Guid id);
    Task<IEnumerable<VirtualMachine>> ListAsync();
    Task StartAsync(Guid id);
    Task StopAsync(Guid id, bool force = false);
    Task PauseAsync(Guid id);
    Task ResumeAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task<VMState> GetStateAsync(Guid id);
}
