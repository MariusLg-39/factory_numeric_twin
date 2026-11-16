namespace ProjetFullstack.Models;

// ============================================================
// Modèle de machine (catalogue partagé par toutes les lignes de production).
// Ex : « Bras robot 6 axes », « Convoyeur droit 2 m ».
// Chaque machine posée dans une ligne est une instance d'un modèle :
// elle reçoit une copie des configurations par défaut, puis vit sa vie.
// ============================================================

public class MachineModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Fichier .glb stocké sur le serveur (ex : "{id}.glb"), null si aucun modèle 3D
    public string? Model3DPath { get; set; }

    // Nom d'origine du fichier envoyé (affichage)
    public string? Model3DFileName { get; set; }

    public List<MachineModelConfiguration> DefaultConfigurations { get; set; } = new List<MachineModelConfiguration>();
    public List<Machine> Machines { get; set; } = new List<Machine>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
