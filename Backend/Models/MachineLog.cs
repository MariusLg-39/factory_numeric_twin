namespace ProjetFullstack.Models;

public class MachineLog
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
}