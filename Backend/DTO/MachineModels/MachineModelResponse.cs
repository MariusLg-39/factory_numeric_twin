namespace ProjetFullstack.DTO.MachineModels;

public class MachineModelResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasModel3D { get; set; }
    public string? Model3DFileName { get; set; }
    public List<DefaultConfigurationResponse> DefaultConfigurations { get; set; } = new();

    // Nombre de machines (toutes lignes confondues) créées à partir de ce modèle
    public int MachineCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DefaultConfigurationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
}
