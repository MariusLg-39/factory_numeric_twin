using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.Products;

public class UpdateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid ProductionLineId { get; set; }
}