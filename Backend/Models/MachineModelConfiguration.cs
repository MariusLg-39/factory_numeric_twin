namespace ProjetFullstack.Models;

// Configuration par défaut d'un modèle de machine.
// Copiée dans MachineConfiguration à chaque nouvelle instance.
public class MachineModelConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; } = 0;
    public string Unit { get; set; } = string.Empty;
    public double MinValue { get; set; } = 0;
    public double MaxValue { get; set; } = 0;

    // Ordre d'affichage
    public int SortOrder { get; set; } = 0;

    public Guid MachineModelId { get; set; }
    public MachineModel MachineModel { get; set; } = null!;
}
