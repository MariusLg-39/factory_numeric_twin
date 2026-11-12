using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.Machines;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

// Machines réelles des lignes de production (instances des modèles du catalogue)
[ApiController]
[Authorize]
[Route("api/machines")]
public class MachinesController : ControllerBase
{
    private readonly IMachineService _service;

    public MachinesController(IMachineService service)
    {
        _service = service;
    }

    // GET /api/machines?productionLineId=... : machines d'une ligne (sans filtre : toute l'usine)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MachineResponse>>> GetAll([FromQuery] Guid? productionLineId)
    {
        var machines = await _service.GetAllAsync(productionLineId);
        return Ok(machines.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MachineResponse>> GetById(Guid id)
    {
        var machine = await _service.GetByIdAsync(id);

        if (machine is null)
            return NotFound();

        return Ok(ToResponse(machine));
    }

    // Pose une nouvelle machine dans une ligne à partir d'un modèle du catalogue
    [HttpPost]
    public async Task<ActionResult<MachineResponse>> Create(CreateMachineRequest request)
    {
        var statusError = ValidateStatus(request.Status);
        if (statusError is not null)
            return statusError;

        if (!await _service.ProductionLineExistsAsync(request.ProductionLineId))
            return BadRequest(new { message = "Ligne de production introuvable." });

        if (await _service.FindModelAsync(request.MachineModelId) is null)
            return BadRequest(new { message = "Modèle de machine introuvable." });

        var placement = request.Placement;
        var machine = new Machine
        {
            Name = request.Name?.Trim() ?? string.Empty,
            Status = request.Status ?? MachineStatus.Stopped,
            MachineModelId = request.MachineModelId,
            ProductionLineId = request.ProductionLineId,
            PositionX = placement?.PositionX ?? 0,
            PositionY = placement?.PositionY ?? 0,
            PositionZ = placement?.PositionZ ?? 0,
            RotationY = placement?.RotationY ?? 0,
            Scale = placement?.Scale ?? 1
        };

        var created = await _service.CreateFromModelAsync(machine);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    // Copie une machine avec ses configurations
    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<MachineResponse>> Duplicate(Guid id)
    {
        var copy = await _service.DuplicateAsync(id);

        if (copy is null)
            return NotFound();

        return CreatedAtAction(nameof(GetById), new { id = copy.Id }, ToResponse(copy));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, MachineRequest request)
    {
        var statusError = ValidateStatus(request.Status);
        if (statusError is not null)
            return statusError;

        if (!await _service.ProductionLineExistsAsync(request.ProductionLineId))
            return BadRequest(new { message = "Ligne de production introuvable." });

        var machine = new Machine
        {
            Name = request.Name.Trim(),
            Status = request.Status ?? string.Empty,
            ProductionLineId = request.ProductionLineId
        };

        var updated = await _service.UpdateAsync(id, machine);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // ============================================================
    // PLACEMENT DANS LE JUMEAU 3D
    // ============================================================

    [HttpPut("{id:guid}/placement")]
    public async Task<ActionResult<MachineResponse>> UpdatePlacement(Guid id, MachinePlacementRequest request)
    {
        var machine = await _service.UpdatePlacementAsync(id, request);

        if (machine is null)
            return NotFound();

        return Ok(ToResponse(machine));
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private ActionResult? ValidateStatus(string? status)
    {
        if (status is not null && !MachineStatus.All.Contains(status))
        {
            return BadRequest(new
            {
                message = $"Statut invalide. Valeurs possibles : {string.Join(", ", MachineStatus.All)}."
            });
        }

        return null;
    }

    private static MachineResponse ToResponse(Machine machine)
    {
        return new MachineResponse
        {
            Id = machine.Id,
            Name = machine.Name,
            Status = machine.Status,
            ProductionLineId = machine.ProductionLineId,
            ProductionLineName = machine.ProductionLine?.Name,
            MachineModelId = machine.MachineModelId,
            MachineModelName = machine.MachineModel?.Name,
            Type = machine.MachineModel?.Type ?? string.Empty,
            HasModel3D = machine.MachineModel?.Model3DPath is not null,
            Model3DFileName = machine.MachineModel?.Model3DFileName,
            PositionX = machine.PositionX,
            PositionY = machine.PositionY,
            PositionZ = machine.PositionZ,
            RotationY = machine.RotationY,
            Scale = machine.Scale,
            CreatedAt = machine.CreatedAt,
            UpdatedAt = machine.UpdatedAt
        };
    }
}
