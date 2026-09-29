namespace ProjetFullstack.Models;

public class MachineConfiguration
{
    public Guid Id { get; set; }

    public int Value { get; set; } = 0;
    public string Unit { get; set; } = string.Empty;
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 0;
    public Guid MachineId { get; set; }
}