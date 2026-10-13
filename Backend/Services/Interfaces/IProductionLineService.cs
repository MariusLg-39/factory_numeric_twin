using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IProductionLineService
{
    Task<IEnumerable<ProductionLine>> GetAllAsync();
    Task<ProductionLine?> GetByIdAsync(Guid id);
    Task<ProductionLine> CreateAsync(ProductionLine line);
    Task<bool> UpdateAsync(Guid id, ProductionLine line);
    Task<bool> DeleteAsync(Guid id);
}