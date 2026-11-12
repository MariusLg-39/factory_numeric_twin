using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.MachineModels;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

[ApiController]
[Authorize]
[Route("api/machine-models")]
public class MachineModelsController : ControllerBase
{
    //Taille max d'un modèle 3D envoyé (100 Mo)
    private const long MaxModel3DSize = 100L * 1024 * 1024;

    private readonly IMachineModelService _service;

    public MachineModelsController(IMachineModelService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MachineModelResponse>>> GetAll()
    {
        var models = await _service.GetAllAsync();
        var counts = await _service.CountMachinesAsync();
        return Ok(models.Select(m => ToResponse(m, counts.GetValueOrDefault(m.Id))));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MachineModelResponse>> GetById(Guid id)
    {
        var model = await _service.GetByIdAsync(id);

        if (model is null)
            return NotFound();

        var counts = await _service.CountMachinesAsync();
        return Ok(ToResponse(model, counts.GetValueOrDefault(id)));
    }

    [HttpPost]
    public async Task<ActionResult<MachineModelResponse>> Create(MachineModelRequest request)
    {
        var error = Validate(request);
        if (error is not null)
            return BadRequest(new { message = error });

        var created = await _service.CreateAsync(FromRequest(request));

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created, 0));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MachineModelResponse>> Update(Guid id, MachineModelRequest request)
    {
        var error = Validate(request);
        if (error is not null)
            return BadRequest(new { message = error });

        var updated = await _service.UpdateAsync(id, FromRequest(request));

        if (updated is null)
            return NotFound();

        var counts = await _service.CountMachinesAsync();
        return Ok(ToResponse(updated, counts.GetValueOrDefault(id)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) switch
        {
            DeleteModelResult.NotFound => NotFound(),
            DeleteModelResult.InUse => Conflict(new
            {
                message = "Ce modèle est utilisé par des machines. Supprime-les d'abord des lignes de production."
            }),
            _ => NoContent()
        };
    }

    // ============================================================
    // MODÈLE 3D (.glb)
    // ============================================================

    [HttpPost("{id:guid}/model")]
    [RequestSizeLimit(MaxModel3DSize + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxModel3DSize + 1024 * 1024)]
    public async Task<ActionResult<MachineModelResponse>> UploadModel3D(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Aucun fichier reçu." });

        if (file.Length > MaxModel3DSize)
            return BadRequest(new { message = "Le fichier dépasse 100 Mo." });

        if (!string.Equals(Path.GetExtension(file.FileName), ".glb", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Seuls les fichiers .glb sont acceptés." });

        await using var stream = file.OpenReadStream();

        // Un fichier GLB commence toujours par les 4 octets "glTF"
        var header = new byte[4];
        var read = await stream.ReadAsync(header);
        if (read < 4 || header[0] != 'g' || header[1] != 'l' || header[2] != 'T' || header[3] != 'F')
            return BadRequest(new { message = "Ce fichier n'est pas un modèle GLB valide." });

        stream.Position = 0;

        var model = await _service.SetModel3DAsync(id, stream, file.FileName);

        if (model is null)
            return NotFound();

        var counts = await _service.CountMachinesAsync();
        return Ok(ToResponse(model, counts.GetValueOrDefault(id)));
    }

    [HttpGet("{id:guid}/model")]
    public async Task<IActionResult> GetModel3D(Guid id)
    {
        var fullPath = await _service.GetModel3DFullPathAsync(id);

        if (fullPath is null)
            return NotFound();

        return PhysicalFile(fullPath, "model/gltf-binary", enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}/model")]
    public async Task<IActionResult> DeleteModel3D(Guid id)
    {
        var removed = await _service.RemoveModel3DAsync(id);

        if (!removed)
            return NotFound();

        return NoContent();
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private static string? Validate(MachineModelRequest request)
    {
        foreach (var c in request.DefaultConfigurations)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                return "Chaque configuration par défaut doit avoir un nom.";
            if (c.MinValue > c.MaxValue)
                return $"« {c.Name} » : le minimum doit être inférieur ou égal au maximum.";
            if (c.Value < c.MinValue || c.Value > c.MaxValue)
                return $"« {c.Name} » : la valeur doit être comprise entre {c.MinValue} et {c.MaxValue}.";
        }

        return null;
    }

    private static MachineModel FromRequest(MachineModelRequest request)
    {
        return new MachineModel
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            DefaultConfigurations = request.DefaultConfigurations
                .Select(c => new MachineModelConfiguration
                {
                    Name = c.Name,
                    Value = c.Value,
                    Unit = c.Unit,
                    MinValue = c.MinValue,
                    MaxValue = c.MaxValue
                })
                .ToList()
        };
    }

    private static MachineModelResponse ToResponse(MachineModel model, int machineCount)
    {
        return new MachineModelResponse
        {
            Id = model.Id,
            Name = model.Name,
            Type = model.Type,
            Description = model.Description,
            HasModel3D = model.Model3DPath is not null,
            Model3DFileName = model.Model3DFileName,
            MachineCount = machineCount,
            DefaultConfigurations = model.DefaultConfigurations
                .OrderBy(c => c.SortOrder)
                .Select(c => new DefaultConfigurationResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Value = c.Value,
                    Unit = c.Unit,
                    MinValue = c.MinValue,
                    MaxValue = c.MaxValue
                })
                .ToList(),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
