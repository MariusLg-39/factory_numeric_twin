using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class Model3DStorage : IModel3DStorage
{
    private readonly string _rootPath;

    public Model3DStorage(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Dossier configurable via "Storage:Models3DPath" (relatif au dossier du Backend par défaut)
        var configured = configuration["Storage:Models3DPath"];
        var path = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine("Storage", "Models3D")
            : configured;

        _rootPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(environment.ContentRootPath, path);

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Guid ownerId, Stream content)
    {
        var storedFileName = $"{ownerId}.glb";
        var fullPath = Path.Combine(_rootPath, storedFileName);
        var tempPath = fullPath + ".tmp";

        // Écriture dans un fichier temporaire puis remplacement : pas de fichier à moitié écrit
        await using (var file = File.Create(tempPath))
        {
            await content.CopyToAsync(file);
        }

        File.Move(tempPath, fullPath, overwrite: true);

        return storedFileName;
    }

    public void Delete(string storedFileName)
    {
        var fullPath = GetFullPath(storedFileName);

        if (fullPath is not null)
            File.Delete(fullPath);
    }

    public string? GetFullPath(string storedFileName)
    {
        // Sécurité : on n'accepte qu'un nom de fichier simple, jamais un chemin
        var safeName = Path.GetFileName(storedFileName);
        if (string.IsNullOrEmpty(safeName) || safeName != storedFileName)
            return null;

        var fullPath = Path.Combine(_rootPath, safeName);
        return File.Exists(fullPath) ? fullPath : null;
    }
}
