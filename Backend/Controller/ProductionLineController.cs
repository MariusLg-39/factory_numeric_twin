using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.ProductionLines;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

[ApiController]
[Route("api/productionLine")]
public class ProductionLinesController : ControllerBase
{
    private readonly IProductionLineService _service;

    public ProductionLinesController(IProductionLineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductionLineResponse>>> GetAll()
    {
        var lines = await _service.GetAllAsync();
        return Ok(lines.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductionLineResponse>> GetById(Guid id)
    {
        var line = await _service.GetByIdAsync(id);

        if (line is null)
            return NotFound();

        return Ok(ToResponse(line));
    }

    [HttpPost]
    public async Task<ActionResult<ProductionLineResponse>> Create(
        CreateProductionLineRequest request)
    {
        var line = new ProductionLine
        {
            Name = request.Name,
            Description = request.Description
        };

        var created = await _service.CreateAsync(line);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateProductionLineRequest request)
    {
        var line = new ProductionLine
        {
            Name = request.Name,
            Description = request.Description
        };

        var updated = await _service.UpdateAsync(id, line);

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

    private static ProductionLineResponse ToResponse(ProductionLine line)
    {
        return new ProductionLineResponse
        {
            Id = line.Id,
            Name = line.Name,
            Description = line.Description,
            CreatedAt = line.CreatedAt,
            UpdatedAt = line.UpdatedAt
        };
    }
}