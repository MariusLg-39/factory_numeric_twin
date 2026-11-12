namespace ProjetFullstack.Services.Interfaces;

// Stockage des fichiers de modèles 3D sur le disque du serveur
public interface IModel3DStorage
{
    // Enregistre le fichier et renvoie le nom stocké (ex : "{idDuModèle}.glb")
    Task<string> SaveAsync(Guid ownerId, Stream content);

    void Delete(string storedFileName);

    // Chemin absolu du fichier, ou null s'il n'existe pas
    string? GetFullPath(string storedFileName);
}
