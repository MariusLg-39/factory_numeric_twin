namespace {{Namespace}}.Services;

public interface I{{ClassName}}Service
{
Task<IEnumerable<{{ClassName}}>> GetAllAsync();
Task<{{ClassName}}?> GetByIdAsync(int id);
Task<{{ClassName}}> CreateAsync({{ClassName}} entity);
Task<bool> UpdateAsync({{ClassName}} entity);
Task<bool> DeleteAsync(int id);
}

public class {{ClassName}}Service : I{{ClassName}}Service
{
private readonly I{{ClassName}}Repository _repository;

```
public {{ClassName}}Service(I{{ClassName}}Repository repository)
{
    _repository = repository;
}

public async Task<IEnumerable<{{ClassName}}>> GetAllAsync()
{
    return await _repository.GetAllAsync();
}

public async Task<{{ClassName}}?> GetByIdAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

public async Task<{{ClassName}}> CreateAsync({{ClassName}} entity)
{
    // Ajouter ici les règles métier nécessaires.

    return await _repository.CreateAsync(entity);
}

public async Task<bool> UpdateAsync({{ClassName}} entity)
{
    var existing = await _repository.GetByIdAsync(entity.Id);

    if (existing is null)
        return false;

    // Ajouter ici les règles métier nécessaires.

    return await _repository.UpdateAsync(entity);
}

public async Task<bool> DeleteAsync(int id)
{
    var existing = await _repository.GetByIdAsync(id);

    if (existing is null)
        return false;

    return await _repository.DeleteAsync(id);
}
}
