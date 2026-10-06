namespace ProjetFullstack.Models;

public class MachineConfiguration
{
    public Guid Id { get; set; }

    public int Value { get; set; } = 0;
    public string Unit { get; set; } = string.Empty;
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
}