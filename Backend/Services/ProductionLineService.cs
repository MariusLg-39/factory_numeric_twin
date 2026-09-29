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
            .Include(p => p.Machines)
            .ToListAsync();
    }

    public async Task<ProductionLine?> GetByIdAsync(Guid id)
    {
        return await _context.Set<ProductionLine>()
            .Include(p => p.Machines)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}