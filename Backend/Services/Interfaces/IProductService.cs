using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Guid id, Product product);
    Task<bool> DeleteAsync(Guid id);
}