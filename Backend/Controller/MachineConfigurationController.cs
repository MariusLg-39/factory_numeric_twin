using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.MachineConfigurations;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

// Configurations propres à chaque machine (ex : paramètres d'un bras robot)
[ApiController]
[Authorize]
[Route("api/machines/{machineId:guid}/configurations")]
public class MachineConfigurationsController : ControllerBase
{
    private readonly IMachineConfigurationService _service;

    public MachineConfigurationsController(IMachineConfigurationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MachineConfigurationResponse>>> GetAll(Guid machineId)
    {
        var configurations = await _service.GetByMachineAsync(machineId);

        if (configurations is null)
            return NotFound(new { message = "Machine introuvable." });

        return Ok(configurations.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MachineConfigurationResponse>> GetById(Guid machineId, Guid id)
    {
        var configuration = await _service.GetByIdAsync(machineId, id);

        if (configuration is null)
            return NotFound();

        return Ok(ToResponse(configuration));
    }

    [HttpPost]
    public async Task<ActionResult<MachineConfigurationResponse>> Create(
        Guid machineId,
        MachineConfigurationRequest request)
    {
        var error = Validate(request);
        if (error is not null)
            return BadRequest(new { message = error });

        var created = await _service.CreateAsync(machineId, FromRequest(request));

        if (created is null)
            return NotFound(new { message = "Machine introuvable." });

        return CreatedAtAction(
            nameof(GetById),
            new { machineId, id = created.Id },
            ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MachineConfigurationResponse>> Update(
        Guid machineId,
        Guid id,
        MachineConfigurationRequest request)
    {
        var error = Validate(request);
        if (error is not null)
            return BadRequest(new { message = error });

        var updated = await _service.UpdateAsync(machineId, id, FromRequest(request));

        if (updated is null)
            return NotFound();

        return Ok(ToResponse(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid machineId, Guid id)
    {
        var deleted = await _service.DeleteAsync(machineId, id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static string? Validate(MachineConfigurationRequest request)
    {
        if (request.MinValue > request.MaxValue)
            return "La valeur minimale doit être inférieure ou égale à la valeur maximale.";

        if (request.Value < request.MinValue || request.Value > request.MaxValue)
            return $"La valeur doit être comprise entre {request.MinValue} et {request.MaxValue}.";

        return null;
    }

    private static MachineConfiguration FromRequest(MachineConfigurationRequest request)
    {
        return new MachineConfiguration
        {
            Name = request.Name,
            Value = request.Value,
            Unit = request.Unit,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue
        };
    }

    private static MachineConfigurationResponse ToResponse(MachineConfiguration configuration)
    {
        return new MachineConfigurationResponse
        {
            Id = configuration.Id,
            MachineId = configuration.MachineId,
            Name = configuration.Name,
            Value = configuration.Value,
            Unit = configuration.Unit,
            MinValue = configuration.MinValue,
            MaxValue = configuration.MaxValue,
            CreatedAt = configuration.CreatedAt,
            UpdatedAt = configuration.UpdatedAt
        };
    }
}
