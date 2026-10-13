using ProjetFullstack.DTO.Users;

namespace ProjetFullstack.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();

    Task<UserResponse?> GetByIdAsync(Guid id);

    Task<UserResponse?> GetByEmailAsync(string email);

    Task<(bool Success, string? Error, UserResponse? User)> CreateAsync(CreateUserRequest request);

    Task<(bool Success, string? Error, UserResponse? User)> UpdateAsync(Guid id, UpdateUserRequest request);

    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}
