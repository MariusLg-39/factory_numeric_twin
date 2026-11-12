namespace ProjetFullstack.Models;

// +===========================================================
// Machine réelle d'une ligne de production (instance d'un modèle du catalogue).
// Toutes les machines sont posées dans le jumeau numérique 3D de l'usine,
// rattachées à une ligne, avec leurs propres configurations, capteurs et logs.
// ============================================================

public class Machine
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = MachineStatus.Stopped;

    // Modèle du catalogue (type de machine + modèle 3D)
    public Guid MachineModelId { get; set; }
    public MachineModel MachineModel { get; set; } = null!;

    public double PositionX { get; set; } = 0;
    public double PositionY { get; set; } = 0;
    public double PositionZ { get; set; } = 0;
    public double RotationY { get; set; } = 0;
    public double Scale { get; set; } = 1;

    public List<MachineConfiguration> MachineConfigurations { get; set; } = new List<MachineConfiguration>();
    public List<MachineLog> MachineLogs { get; set; } = new List<MachineLog>();
    public List<Sensor> Sensors { get; set; } = new List<Sensor>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid ProductionLineId { get; set; }
    public ProductionLine ProductionLine { get; set; } = null!;
}

public static class MachineStatus
{
    public const string Running = "Running";
    public const string Stopped = "Stopped";
    public const string Maintenance = "Maintenance";
    public const string Error = "Error";

    public static readonly string[] All = { Running, Stopped, Maintenance, Error };
}
