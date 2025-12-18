using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanVM.Core.Interfaces
{
    public interface IHypervisor
    {
        string Name { get; }
        bool IsAvailable { get; }
        
        Task<IVirtualMachine> CreateVirtualMachineAsync(VMConfiguration config);
        Task<IVirtualMachine> GetVirtualMachineAsync(Guid id);
        Task<IEnumerable<IVirtualMachine>> ListVirtualMachinesAsync();
        Task DeleteVirtualMachineAsync(Guid id);
        
        Task<HypervisorCapabilities> GetCapabilitiesAsync();
    }
    
    public class HypervisorCapabilities
    {
        public bool SupportsNesting { get; set; }
        public bool SupportsSnapshots { get; set; }
        public long MaxMemoryMB { get; set; }
        public int MaxCPUs { get; set; }
    }
}
