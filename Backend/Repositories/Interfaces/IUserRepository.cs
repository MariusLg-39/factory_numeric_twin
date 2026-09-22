using ProjetFullstack.Models;

namespace ProjetFullstack.Repositories.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id);

    Task<ApplicationUser?> GetByEmailAsync(string email);

    Task<IReadOnlyList<ApplicationUser>> GetAllAsync();

    Task<bool> ExistsByEmailAsync(string email);
}
