using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.Machines;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

[ApiController]
[Route("api/machines")]
public class MachinesController : ControllerBase
{
    private readonly IMachineService _service;

    public MachinesController(IMachineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MachineResponse>>> GetAll()
    {
        var machines = await _service.GetAllAsync();
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

    [HttpPost]
    public async Task<ActionResult<MachineResponse>> Create(
        MachineRequest request)
    {
        var machine = new Machine
        {
            Name = request.Name,
            Type = request.Type,
            ProductionLineId = request.ProductionLineId
        };

        var created = await _service.CreateAsync(machine);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        MachineRequest request)
    {
        var machine = new Machine
        {
            Name = request.Name,
            Type = request.Type,
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

    private static MachineResponse ToResponse(Machine machine)
    {
        return new MachineResponse
        {
            Id = machine.Id,
            Name = machine.Name,
            Type = machine.Type,
            ProductionLineId = machine.ProductionLineId
        };
    }
}