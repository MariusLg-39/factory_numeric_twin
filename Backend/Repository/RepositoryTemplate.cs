using Microsoft.EntityFrameworkCore;

namespace {{Namespace}}.Repositories;

public interface I{{ClassName}}Repository
{
Task<IEnumerable<{{ClassName}}>> GetAllAsync();
Task<{{ClassName}}?> GetByIdAsync(int id);
Task<{{ClassName}}> CreateAsync({{ClassName}} entity);
Task<bool> UpdateAsync({{ClassName}} entity);
Task<bool> DeleteAsync(int id);
}

public class {{ClassName}}Repository : I{{ClassName}}Repository
{
private readonly {{DbContextName}} _context;

```
public {{ClassName}}Repository({{DbContextName}} context)
{
    _context = context;
}

public async Task<IEnumerable<{{ClassName}}>> GetAllAsync()
{
    return await _context.Set<{{ClassName}}>()
        .AsNoTracking()
        .ToListAsync();
}

public async Task<{{ClassName}}?> GetByIdAsync(int id)
{
    return await _context.Set<{{ClassName}}>()
        .FirstOrDefaultAsync(x => x.Id == id);
}

public async Task<{{ClassName}}> CreateAsync({{ClassName}} entity)
{
    await _context.Set<{{ClassName}}>().AddAsync(entity);
    await _context.SaveChangesAsync();

    return entity;
}

public async Task<bool> UpdateAsync({{ClassName}} entity)
{
    _context.Set<{{ClassName}}>().Update(entity);

    await _context.SaveChangesAsync();

    return true;
}

public async Task<bool> DeleteAsync(int id)
{
    var entity = await _context.Set<{{ClassName}}>()
        .FirstOrDefaultAsync(x => x.Id == id);

    if (entity is null)
        return false;

    _context.Set<{{ClassName}}>().Remove(entity);

    await _context.SaveChangesAsync();

    return true;
}

}
