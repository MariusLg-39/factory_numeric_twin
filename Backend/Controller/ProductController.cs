using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.Products;
using ProjetFullstack.Models;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();

        var response = products.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            ProductionLineId = p.ProductionLineId
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        return Ok(new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            ProductionLineId = product.ProductionLineId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            ProductionLineId = request.ProductionLineId
        };

        var created = await _productService.CreateAsync(product);

        var response = new ProductResponse
        {
            Id = created.Id,
            Name = created.Name,
            ProductionLineId = created.ProductionLineId
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            ProductionLineId = request.ProductionLineId
        };

        var updated = await _productService.UpdateAsync(id, product);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _productService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}