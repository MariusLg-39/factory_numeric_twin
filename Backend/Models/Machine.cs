namespace ProjetFullstack.Models;

public class Machine
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid ProductionLineId { get; set; }

    public ProductionLine ProductionLine { get; set; } = null!;
}