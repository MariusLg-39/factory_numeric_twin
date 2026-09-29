using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IProductionLineService
{
    Task<IEnumerable<ProductionLine>> GetAllAsync();

    Task<ProductionLine?> GetByIdAsync(Guid id);
}