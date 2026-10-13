using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.MachineLogs;

public class MachineLogRequest
{
    [Required]
    public Guid MachineId { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}