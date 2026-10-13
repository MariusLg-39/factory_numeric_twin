using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class ProductionLineService : IProductionLineService
{
    private readonly ApplicationDbContext _context;

    public ProductionLineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductionLine>> GetAllAsync()
    {
        return await _context.Set<ProductionLine>()
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ProductionLine?> GetByIdAsync(Guid id)
    {
        return await _context.Set<ProductionLine>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProductionLine> CreateAsync(ProductionLine line)
    {
        line.Id = Guid.NewGuid();
        line.CreatedAt = DateTime.UtcNow;
        line.UpdatedAt = DateTime.UtcNow;

        _context.Set<ProductionLine>().Add(line);
        await _context.SaveChangesAsync();

        return line;
    }

    public async Task<bool> UpdateAsync(Guid id, ProductionLine line)
    {
        var existing = await _context.Set<ProductionLine>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existing is null)
            return false;

        existing.Name = line.Name;
        existing.Description = line.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _context.Set<ProductionLine>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existing is null)
            return false;

        _context.Set<ProductionLine>().Remove(existing);
        await _context.SaveChangesAsync();

        return true;
    }
}