using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class MachineService : IMachineService
{
    private readonly ApplicationDbContext _context;

    public MachineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Machine>> GetAllAsync()
    {
        return await _context.Set<Machine>()
            .Include(m => m.ProductionLine)
            .ToListAsync();
    }

    public async Task<Machine?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Machine>()
            .Include(m => m.ProductionLine)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Machine> CreateAsync(Machine machine)
    {
        machine.Id = Guid.NewGuid();

        _context.Set<Machine>().Add(machine);
        await _context.SaveChangesAsync();

        return machine;
    }

    public async Task<bool> UpdateAsync(Guid id, Machine machine)
    {
        var existing = await _context.Set<Machine>()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existing == null)
            return false;

        existing.Name = machine.Name;
        existing.Type = machine.Type;
        existing.ProductionLineId = machine.ProductionLineId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var machine = await _context.Set<Machine>()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (machine == null)
            return false;

        _context.Set<Machine>().Remove(machine);
        await _context.SaveChangesAsync();

        return true;
    }
}