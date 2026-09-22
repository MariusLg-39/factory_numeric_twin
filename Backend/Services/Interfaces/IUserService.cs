using ProjetFullstack.DTO.Users;

namespace ProjetFullstack.Services.Interfaces;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid id);

    Task<UserResponse?> GetByEmailAsync(string email);

    Task<IReadOnlyList<UserResponse>> GetAllAsync();

}
