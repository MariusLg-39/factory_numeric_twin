using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Set<Product>()
            .Include(p => p.ProductionLine)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Product>()
            .Include(p => p.ProductionLine)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.Id = Guid.NewGuid();

        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<bool> UpdateAsync(Guid id, Product product)
    {
        var existing = await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existing == null)
            return false;

        existing.Name = product.Name;
        existing.ProductionLineId = product.ProductionLineId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return false;

        _context.Set<Product>().Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }
}