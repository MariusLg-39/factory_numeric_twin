using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.MachineConfigurations;

public class MachineConfigurationRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public double Value { get; set; }

    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    public double MinValue { get; set; }

    public double MaxValue { get; set; }
}
