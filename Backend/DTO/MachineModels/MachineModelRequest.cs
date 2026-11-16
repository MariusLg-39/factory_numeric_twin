using System.ComponentModel.DataAnnotations;
using ProjetFullstack.DTO.MachineConfigurations;

namespace ProjetFullstack.DTO.MachineModels;

public class MachineModelRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string Type { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    // Configurations par défaut (remplacent les existantes à la modification)
    public List<MachineConfigurationRequest> DefaultConfigurations { get; set; } = new();
}
