namespace ProjetFullstack.DTO.Products;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ProductionLineId { get; set; }
}