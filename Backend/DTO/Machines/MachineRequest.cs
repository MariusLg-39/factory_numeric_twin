using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.Machines;

public class MachineRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Type { get; set; } = string.Empty;

    [Required]
    public Guid ProductionLineId { get; set; }
}