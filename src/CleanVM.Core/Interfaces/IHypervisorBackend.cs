using CleanVM.Core.Models;

namespace CleanVM.Core.Interfaces;

public interface IHypervisorBackend
{
    Task<bool> IsAvailableAsync();
    Task<string> CreateVMAsync(VMConfiguration config, string name);
    Task StartVMAsync(string vmId);
    Task StopVMAsync(string vmId, bool force);
    Task PauseVMAsync(string vmId);
    Task ResumeVMAsync(string vmId);
    Task DeleteVMAsync(string vmId);
    Task<VMState> GetStateAsync(string vmId);
    Task<string> GetVNCAddressAsync(string vmId);
}
