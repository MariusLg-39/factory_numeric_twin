namespace ProjetFullstack.Models;

public class MachineConfiguration
{
    public Guid Id { get; set; }

    // Nom du paramètre (ex : "Vitesse de rotation", "Couple max de l'axe 3")
    public string Name { get; set; } = string.Empty;

    public double Value { get; set; } = 0;
    public string Unit { get; set; } = string.Empty;
    public double MinValue { get; set; } = 0;
    public double MaxValue { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
}
