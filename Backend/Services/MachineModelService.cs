using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class MachineModelService : IMachineModelService
{
    private readonly ApplicationDbContext _context;
    private readonly IModel3DStorage _storage;

    public MachineModelService(ApplicationDbContext context, IModel3DStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<IEnumerable<MachineModel>> GetAllAsync()
    {
        return await _context.Set<MachineModel>()
            .Include(m => m.DefaultConfigurations.OrderBy(c => c.SortOrder))
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<MachineModel?> GetByIdAsync(Guid id)
    {
        return await _context.Set<MachineModel>()
            .Include(m => m.DefaultConfigurations.OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Dictionary<Guid, int>> CountMachinesAsync()
    {
        return await _context.Set<Machine>()
            .GroupBy(m => m.MachineModelId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    public async Task<MachineModel> CreateAsync(MachineModel model)
    {
        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;

        var order = 0;
        foreach (var configuration in model.DefaultConfigurations)
        {
            configuration.Id = Guid.NewGuid();
            configuration.MachineModelId = model.Id;
            configuration.SortOrder = order++;
        }

        _context.Set<MachineModel>().Add(model);
        await _context.SaveChangesAsync();

        return model;
    }

    public async Task<MachineModel?> UpdateAsync(Guid id, MachineModel model)
    {
        var existing = await GetByIdAsync(id);

        if (existing == null)
            return null;

        existing.Name = model.Name;
        existing.Type = model.Type;
        existing.Description = model.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        // Les configurations par défaut sont remplacées.
        // Les machines déjà créées gardent leurs propres configurations.
        _context.Set<MachineModelConfiguration>().RemoveRange(existing.DefaultConfigurations);

        var order = 0;
        existing.DefaultConfigurations = model.DefaultConfigurations
            .Select(c => new MachineModelConfiguration
            {
                Id = Guid.NewGuid(),
                MachineModelId = existing.Id,
                Name = c.Name,
                Value = c.Value,
                Unit = c.Unit,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue,
                SortOrder = order++
            })
            .ToList();

        _context.Set<MachineModelConfiguration>().AddRange(existing.DefaultConfigurations);
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<DeleteModelResult> DeleteAsync(Guid id)
    {
        var model = await _context.Set<MachineModel>()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (model == null)
            return DeleteModelResult.NotFound;

        // Un modèle utilisé par des machines ne peut pas être supprimé
        if (await _context.Set<Machine>().AnyAsync(m => m.MachineModelId == id))
            return DeleteModelResult.InUse;

        var modelPath = model.Model3DPath;

        _context.Set<MachineModel>().Remove(model);
        await _context.SaveChangesAsync();

        if (modelPath is not null)
            _storage.Delete(modelPath);

        return DeleteModelResult.Deleted;
    }

    public async Task<MachineModel?> SetModel3DAsync(Guid id, Stream content, string originalFileName)
    {
        var model = await GetByIdAsync(id);

        if (model == null)
            return null;

        model.Model3DPath = await _storage.SaveAsync(model.Id, content);
        model.Model3DFileName = Path.GetFileName(originalFileName);
        model.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return model;
    }

    public async Task<bool> RemoveModel3DAsync(Guid id)
    {
        var model = await _context.Set<MachineModel>()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (model == null)
            return false;

        if (model.Model3DPath is not null)
            _storage.Delete(model.Model3DPath);

        model.Model3DPath = null;
        model.Model3DFileName = null;
        model.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GetModel3DFullPathAsync(Guid id)
    {
        var storedFileName = await _context.Set<MachineModel>()
            .Where(m => m.Id == id)
            .Select(m => m.Model3DPath)
            .FirstOrDefaultAsync();

        return storedFileName is null ? null : _storage.GetFullPath(storedFileName);
    }
}
