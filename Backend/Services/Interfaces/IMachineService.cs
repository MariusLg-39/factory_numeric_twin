using ProjetFullstack.DTO.Machines;
using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IMachineService
{
    Task<IEnumerable<Machine>> GetAllAsync(Guid? productionLineId = null);
    Task<Machine?> GetByIdAsync(Guid id);
    Task<Machine> CreateFromModelAsync(Machine machine);
    Task<Machine?> DuplicateAsync(Guid id);

    Task<bool> UpdateAsync(Guid id, Machine machine);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ProductionLineExistsAsync(Guid productionLineId);
    Task<MachineModel?> FindModelAsync(Guid machineModelId);

    Task<Machine?> UpdatePlacementAsync(Guid id, MachinePlacementRequest placement);
}
