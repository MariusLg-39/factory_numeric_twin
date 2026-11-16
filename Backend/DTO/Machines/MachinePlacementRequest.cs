using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.Machines;

// Position d'une machine dans l'éditeur 3D (mètres / degrés)
public class MachinePlacementRequest
{
    [Range(-10000, 10000)]
    public double PositionX { get; set; }

    [Range(-10000, 10000)]
    public double PositionY { get; set; }

    [Range(-10000, 10000)]
    public double PositionZ { get; set; }

    [Range(-360, 360)]
    public double RotationY { get; set; }

    [Range(0.001, 1000)]
    public double Scale { get; set; } = 1;
}
