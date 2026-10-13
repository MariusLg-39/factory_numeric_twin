using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IMachineLogService
{
    Task<IEnumerable<MachineLog>> GetAllAsync();
    Task<MachineLog?> GetByIdAsync(Guid id);
    Task<MachineLog> CreateAsync(MachineLog machineLog);
    Task<bool> UpdateAsync(Guid id, MachineLog machineLog);
    Task<bool> DeleteAsync(Guid id);
}