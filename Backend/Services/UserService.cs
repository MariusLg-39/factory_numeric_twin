
using Microsoft.AspNetCore.Identity;
using ProjetFullstack.Data;
using ProjetFullstack.DTO.Users;
using ProjetFullstack.Models;
using ProjetFullstack.Repositories.Interfaces;
using ProjetFullstack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ProjetFullstack.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserService(
        IUserRepository repository,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _repository = repository;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();

        var repsonses = new List<UserResponse>();

        foreach (var user in users)
        {
            repsonses.Add(await MapToResponse(user));
        }

        return repsonses;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return null;

        return await MapToResponse(user);
    }

    public async Task<UserResponse?> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email);

        if (user is null)
            return null;

        return await MapToResponse(user);
    }

    public async Task<(bool Success, string? Error, UserResponse? User)>
        CreateAsync(CreateUserRequest request)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                var errors = string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description));

                return (false, errors, null);
            }

            var role = request.IsAdmin ? "Admin" : "User";

            var roleResult = await _userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(e => e.Description));

                return (false, errors, null);
            }

            await transaction.CommitAsync();

            return (true, null, await MapToResponse(user));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Success, string? Error, UserResponse? User)> UpdateAsync(
        Guid id,
        UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(
            id.ToString()
        );

        if (user is null)
        {
            return (
                false,
                "Utilisateur introuvable.",
                null
            );
        }

        var existingUser = await _repository.GetByEmailAsync(
            request.Email
        );

        if (existingUser is not null && existingUser.Id != id)
        {
            return (
                false,
                "Cet email est déjà utilisé.",
                null
            );
        }

        user.Email = request.Email;
        user.UserName = request.UserName;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        user.IsActive = request.IsActive;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description)
            );

            return (
                false,
                errors,
                null
            );
        }

        return (
            true,
            null,
            await MapToResponse(user)
        );
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(
        Guid id)
    {
        var user = await _userManager.FindByIdAsync(
            id.ToString()
        );

        if (user is null)
        {
            return (
                false,
                "Utilisateur introuvable."
            );
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description)
            );

            return (
                false,
                errors
            );
        }

        return (
            true,
            null
        );
    }

    private async Task<UserResponse> MapToResponse(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        };
    }
}
