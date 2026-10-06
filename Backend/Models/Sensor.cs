namespace ProjetFullstack.Models;

public class Sensor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<Measure> Measures { get; set; } = new List<Measure>();
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
}