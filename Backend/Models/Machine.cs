namespace ProjetFullstack.Models;

public class Machine
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<MachineConfiguration> MachineConfigurations { get; set; } = new List<MachineConfiguration>();
    public List<Sensor> Sensors { get; set; } = new List<Sensor>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public Guid ProductionLineId { get; set; }
    public ProductionLine ProductionLine { get; set; } = null!;
}