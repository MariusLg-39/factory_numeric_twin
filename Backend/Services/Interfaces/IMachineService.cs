using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IMachineService
{
    Task<IEnumerable<Machine>> GetAllAsync();
    Task<Machine?> GetByIdAsync(Guid id);
    Task<Machine> CreateAsync(Machine machine);
    Task<bool> UpdateAsync(Guid id, Machine machine);
    Task<bool> DeleteAsync(Guid id);
}