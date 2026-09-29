using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

[ApiController]
[Route("api/productionLine")]
public class ProductionLineController : ControllerBase
{
    private readonly IProductionLineService _productionLineService;

    public ProductionLineController(IProductionLineService productionLineService)
    {
        _productionLineService = productionLineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPL()
    {
        var productionLines = await _productionLineService.GetAllAsync();

        if(productionLines == null){
            return NotFound();
        }

        return Ok(productionLines);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPLById(Guid id)
    {
        var productionLine = await _productionLineService.GetByIdAsync(id);

        if (productionLine is null)
            return NotFound();

        return Ok(productionLine);
    }
}
