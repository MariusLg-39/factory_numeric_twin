using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class MachineLogService : IMachineLogService
{
    private readonly ApplicationDbContext _context;

    public MachineLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MachineLog>> GetAllAsync()
    {
        return await _context.Set<MachineLog>()
            .Include(log => log.Machine)
            .ToListAsync();
    }

    public async Task<MachineLog?> GetByIdAsync(Guid id)
    {
        return await _context.Set<MachineLog>()
            .Include(log => log.Machine)
            .FirstOrDefaultAsync(log => log.Id == id);
    }

    public async Task<MachineLog> CreateAsync(MachineLog machineLog)
    {
        machineLog.Id = Guid.NewGuid();

        _context.Set<MachineLog>().Add(machineLog);
        await _context.SaveChangesAsync();

        return machineLog;
    }

    public async Task<bool> UpdateAsync(Guid id, MachineLog machineLog)
    {
        var existing = await _context.Set<MachineLog>()
            .FirstOrDefaultAsync(log => log.Id == id);

        if (existing == null)
            return false;

        existing.MachineId = machineLog.MachineId;
        existing.CreatedAt = machineLog.CreatedAt;
        existing.Message = machineLog.Message;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var machineLog = await _context.Set<MachineLog>()
            .FirstOrDefaultAsync(log => log.Id == id);

        if (machineLog == null)
            return false;

        _context.Set<MachineLog>().Remove(machineLog);
        await _context.SaveChangesAsync();

        return true;
    }
}