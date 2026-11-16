using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class MachineConfigurationService : IMachineConfigurationService
{
    private readonly ApplicationDbContext _context;

    public MachineConfigurationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MachineConfiguration>?> GetByMachineAsync(Guid machineId)
    {
        if (!await MachineExistsAsync(machineId))
            return null;

        return await _context.Set<MachineConfiguration>()
            .Where(c => c.MachineId == machineId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<MachineConfiguration?> GetByIdAsync(Guid machineId, Guid configurationId)
    {
        return await _context.Set<MachineConfiguration>()
            .FirstOrDefaultAsync(c => c.Id == configurationId && c.MachineId == machineId);
    }

    public async Task<MachineConfiguration?> CreateAsync(Guid machineId, MachineConfiguration configuration)
    {
        if (!await MachineExistsAsync(machineId))
            return null;

        configuration.Id = Guid.NewGuid();
        configuration.MachineId = machineId;
        configuration.CreatedAt = DateTime.UtcNow;
        configuration.UpdatedAt = DateTime.UtcNow;

        _context.Set<MachineConfiguration>().Add(configuration);
        await _context.SaveChangesAsync();

        return configuration;
    }

    public async Task<MachineConfiguration?> UpdateAsync(Guid machineId, Guid configurationId, MachineConfiguration configuration)
    {
        var existing = await GetByIdAsync(machineId, configurationId);

        if (existing == null)
            return null;

        existing.Name = configuration.Name;
        existing.Value = configuration.Value;
        existing.Unit = configuration.Unit;
        existing.MinValue = configuration.MinValue;
        existing.MaxValue = configuration.MaxValue;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid machineId, Guid configurationId)
    {
        var existing = await GetByIdAsync(machineId, configurationId);

        if (existing == null)
            return false;

        _context.Set<MachineConfiguration>().Remove(existing);
        await _context.SaveChangesAsync();

        return true;
    }

    private Task<bool> MachineExistsAsync(Guid machineId)
    {
        return _context.Set<Machine>().AnyAsync(m => m.Id == machineId);
    }
}
