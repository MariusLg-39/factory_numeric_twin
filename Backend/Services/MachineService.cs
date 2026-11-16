using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.DTO.Machines;
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

    private IQueryable<Machine> WithDetails()
    {
        return _context.Set<Machine>()
            .Include(m => m.ProductionLine)
            .Include(m => m.MachineModel);
    }

    public async Task<IEnumerable<Machine>> GetAllAsync(Guid? productionLineId = null)
    {
        var query = WithDetails();

        if (productionLineId is not null)
            query = query.Where(m => m.ProductionLineId == productionLineId);

        return await query
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Machine?> GetByIdAsync(Guid id)
    {
        return await WithDetails().FirstOrDefaultAsync(m => m.Id == id);
    }

    public Task<MachineModel?> FindModelAsync(Guid machineModelId)
    {
        return _context.Set<MachineModel>()
            .Include(m => m.DefaultConfigurations)
            .FirstOrDefaultAsync(m => m.Id == machineModelId);
    }

    public Task<bool> ProductionLineExistsAsync(Guid productionLineId)
    {
        return _context.Set<ProductionLine>().AnyAsync(pl => pl.Id == productionLineId);
    }

    public async Task<Machine> CreateFromModelAsync(Machine machine)
    {
        var model = await FindModelAsync(machine.MachineModelId)
            ?? throw new InvalidOperationException("Modèle de machine introuvable.");

        machine.Id = Guid.NewGuid();
        machine.CreatedAt = DateTime.UtcNow;
        machine.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(machine.Status))
            machine.Status = MachineStatus.Stopped;

        // Nom automatique : « Bras robot 6 axes 3 »
        if (string.IsNullOrWhiteSpace(machine.Name))
            machine.Name = await NextNameAsync(model.Name);

        // Chaque machine reçoit sa propre copie des configurations par défaut
        machine.MachineConfigurations = model.DefaultConfigurations
            .OrderBy(c => c.SortOrder)
            .Select((c, index) => new MachineConfiguration
            {
                Id = Guid.NewGuid(),
                MachineId = machine.Id,
                Name = c.Name,
                Value = c.Value,
                Unit = c.Unit,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue,
                // Décalage d'1 ms
                CreatedAt = DateTime.UtcNow.AddMilliseconds(index),
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        _context.Set<Machine>().Add(machine);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(machine.Id))!;
    }

    public async Task<Machine?> DuplicateAsync(Guid id)
    {
        var source = await _context.Set<Machine>()
            .Include(m => m.MachineConfigurations)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (source == null)
            return null;

        var copy = new Machine
        {
            Id = Guid.NewGuid(),
            Name = await NextNameAsync(BaseName(source.Name)),
            Status = source.Status,
            MachineModelId = source.MachineModelId,
            ProductionLineId = source.ProductionLineId,
            PositionX = source.PositionX + 1.5,
            PositionY = source.PositionY,
            PositionZ = source.PositionZ + 1.5,
            RotationY = source.RotationY,
            Scale = source.Scale,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        copy.MachineConfigurations = source.MachineConfigurations
            .OrderBy(c => c.CreatedAt)
            .Select((c, index) => new MachineConfiguration
            {
                Id = Guid.NewGuid(),
                MachineId = copy.Id,
                Name = c.Name,
                Value = c.Value,
                Unit = c.Unit,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue,
                // Décalage d'1 ms
                CreatedAt = DateTime.UtcNow.AddMilliseconds(index),
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        _context.Set<Machine>().Add(copy);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(copy.Id);
    }

    public async Task<bool> UpdateAsync(Guid id, Machine machine)
    {
        var existing = await _context.Set<Machine>()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existing == null)
            return false;

        existing.Name = machine.Name;
        existing.ProductionLineId = machine.ProductionLineId;

        if (!string.IsNullOrWhiteSpace(machine.Status))
            existing.Status = machine.Status;

        existing.UpdatedAt = DateTime.UtcNow;

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

    public async Task<Machine?> UpdatePlacementAsync(Guid id, MachinePlacementRequest placement)
    {
        var machine = await GetByIdAsync(id);

        if (machine == null)
            return null;

        machine.PositionX = placement.PositionX;
        machine.PositionY = placement.PositionY;
        machine.PositionZ = placement.PositionZ;
        machine.RotationY = placement.RotationY;
        machine.Scale = placement.Scale;
        machine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return machine;
    }

    // ============================================================
    // NOMS AUTOMATIQUES
    // ============================================================

    private static string BaseName(string name)
    {
        var trimmed = name.TrimEnd();
        var lastSpace = trimmed.LastIndexOf(' ');
        return lastSpace > 0 && int.TryParse(trimmed[(lastSpace + 1)..], out _)
            ? trimmed[..lastSpace]
            : trimmed;
    }

    private async Task<string> NextNameAsync(string baseName)
    {
        var names = await _context.Set<Machine>()
            .Where(m => m.Name.StartsWith(baseName))
            .Select(m => m.Name)
            .ToListAsync();

        var taken = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        var number = 1;
        string candidate;
        do
        {
            candidate = $"{baseName} {number++}";
        } while (taken.Contains(candidate));

        return candidate.Length <= 100 ? candidate : candidate[..100];
    }
}
