using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.MachineLogs;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

[ApiController]
[Route("api/machineLog")]
public class MachineLogsController : ControllerBase
{
    private readonly IMachineLogService _service;

    public MachineLogsController(IMachineLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MachineLogResponse>>> GetAll()
    {
        var logs = await _service.GetAllAsync();
        return Ok(logs.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MachineLogResponse>> GetById(Guid id)
    {
        var log = await _service.GetByIdAsync(id);

        if (log is null)
            return NotFound();

        return Ok(ToResponse(log));
    }

    [HttpPost]
    public async Task<ActionResult<MachineLogResponse>> Create(
        MachineLogRequest request)
    {
        var log = new MachineLog
        {
            MachineId = request.MachineId,
            CreatedAt = request.CreatedAt,
            Message = request.Message
        };

        var created = await _service.CreateAsync(log);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        MachineLogRequest request)
    {
        var log = new MachineLog
        {
            MachineId = request.MachineId,
            CreatedAt = request.CreatedAt,
            Message = request.Message
        };

        var updated = await _service.UpdateAsync(id, log);

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

    private static MachineLogResponse ToResponse(MachineLog log)
    {
        return new MachineLogResponse
        {
            Id = log.Id,
            MachineId = log.MachineId,
            CreatedAt = log.CreatedAt,
            Message = log.Message
        };
    }
}