using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IMachineConfigurationService
{
    // null si la machine n'existe pas
    Task<IEnumerable<MachineConfiguration>?> GetByMachineAsync(Guid machineId);
    Task<MachineConfiguration?> GetByIdAsync(Guid machineId, Guid configurationId);

    // null si la machine n'existe pas
    Task<MachineConfiguration?> CreateAsync(Guid machineId, MachineConfiguration configuration);
    Task<MachineConfiguration?> UpdateAsync(Guid machineId, Guid configurationId, MachineConfiguration configuration);
    Task<bool> DeleteAsync(Guid machineId, Guid configurationId);
}
