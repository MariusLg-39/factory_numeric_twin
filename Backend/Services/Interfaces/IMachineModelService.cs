using ProjetFullstack.Models;

namespace ProjetFullstack.Services.Interfaces;

public enum DeleteModelResult
{
    Deleted,
    NotFound,
    InUse,
}

public interface IMachineModelService
{
    Task<IEnumerable<MachineModel>> GetAllAsync();
    Task<MachineModel?> GetByIdAsync(Guid id);
    Task<Dictionary<Guid, int>> CountMachinesAsync();
    Task<MachineModel> CreateAsync(MachineModel model);

    // Remplace aussi les configurations par défaut
    Task<MachineModel?> UpdateAsync(Guid id, MachineModel model);
    Task<DeleteModelResult> DeleteAsync(Guid id);

    // Modèle 3D (.glb)
    Task<MachineModel?> SetModel3DAsync(Guid id, Stream content, string originalFileName);
    Task<bool> RemoveModel3DAsync(Guid id);
    Task<string?> GetModel3DFullPathAsync(Guid id);
}
