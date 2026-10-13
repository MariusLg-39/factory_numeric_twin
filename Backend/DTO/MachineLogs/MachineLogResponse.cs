namespace ProjetFullstack.DTO.MachineLogs;

public class MachineLogResponse
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}