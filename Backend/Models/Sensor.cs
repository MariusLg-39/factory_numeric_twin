namespace ProjetFullstack.Models;

public class Sensor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid MachineId { get; set; }
}