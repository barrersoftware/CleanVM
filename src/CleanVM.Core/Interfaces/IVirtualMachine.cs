using System;
using System.Threading.Tasks;

namespace CleanVM.Core.Interfaces
{
    public interface IVirtualMachine
    {
        Guid Id { get; }
        string Name { get; }
        VirtualMachineState State { get; }
        VMConfiguration Configuration { get; }
        
        Task StartAsync();
        Task StopAsync(bool force = false);
        Task PauseAsync();
        Task ResumeAsync();
        Task ResetAsync();
        Task<VirtualMachineSnapshot> CreateSnapshotAsync(string name);
        Task RestoreSnapshotAsync(Guid snapshotId);
    }
    
    public enum VirtualMachineState
    {
        Stopped,
        Starting,
        Running,
        Paused,
        Stopping,
        Error
    }

    public record VirtualMachineSnapshot(
        Guid Id,
        string Name,
        DateTime CreatedAt
    );
}
