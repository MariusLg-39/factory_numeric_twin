namespace ProjetFullstack.DTO.Machines;

public class MachineResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid ProductionLineId { get; set; }
    public string? ProductionLineName { get; set; }

    //Modèle du catalogue
    public Guid MachineModelId { get; set; }
    public string? MachineModelName { get; set; }
    public string Type { get; set; } = string.Empty; //type du modèle
    public bool HasModel3D { get; set; }
    public string? Model3DFileName { get; set; }

    //Placement dans le jumeau 3D
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public double RotationY { get; set; }
    public double Scale { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
