namespace ProjetFullstack.Models;

public class Measure
{
    public Guid Id { get; set; }
    public int Value { get; set; } = 0;
    public string Unit { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid SensorId { get; set; }
    public Sensor Sensor { get; set; } = null!;
}