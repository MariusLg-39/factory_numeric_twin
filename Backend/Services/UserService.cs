using ProjetFullstack.DTO.Users;
using ProjetFullstack.Repositories.Interfaces;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;


    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return null;

        return MapToResponse(user);
    }

    public async Task<UserResponse?> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email);

        if (user is null)
            return null;

        return MapToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();

        return users
            .Select(MapToResponse)
            .ToList();
    }

    private static UserResponse MapToResponse(Models.ApplicationUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
