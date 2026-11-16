using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.Machines;

// Création d'une machine dans une ligne, à partir d'un modèle du catalogue
public class CreateMachineRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    public Guid MachineModelId { get; set; }

    [Required]
    public Guid ProductionLineId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public MachinePlacementRequest? Placement { get; set; }
}

// Modification d'une machine existante
public class MachineRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Status { get; set; }

    [Required]
    public Guid ProductionLineId { get; set; }
}
