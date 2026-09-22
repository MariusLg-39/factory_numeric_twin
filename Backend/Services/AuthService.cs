using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using ProjetFullstack.Authentication;
using ProjetFullstack.DTO.Auth;
using ProjetFullstack.Models;
using ProjetFullstack.Repositories.Interfaces;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Services;

public class AuthService : IAuthService
{
private readonly UserManager<ApplicationUser> _userManager;
private readonly SignInManager<ApplicationUser> _signInManager;
private readonly IRefreshTokenRepository _refreshTokenRepository;
private readonly JwtService _jwtService;

private const int RefreshTokenExpirationDays = 30;

public AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IRefreshTokenRepository refreshTokenRepository,
    JwtService jwtService)
{
    _userManager = userManager;
    _signInManager = signInManager;
    _refreshTokenRepository = refreshTokenRepository;
    _jwtService = jwtService;
}

public async Task<AuthResponse> RegisterAsync(
    RegisterRequest request)
{
    var existingUser = await _userManager
        .FindByEmailAsync(request.Email);

    if (existingUser is not null)
    {
        throw new InvalidOperationException("Unable to create the account.");
    }

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName
    };

    var result = await _userManager
        .CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
        var errors = string.Join(
            ", ",
            result.Errors.Select(x => x.Description));

        throw new InvalidOperationException(errors);
    }

    await _userManager.AddToRoleAsync(user, "User");

    return await CreateAuthResponseAsync(user);
}

public async Task<AuthResponse> LoginAsync(
    LoginRequest request)
{
    var user = await _userManager
        .FindByEmailAsync(request.Email);

    if (user is null || !user.IsActive)
    {
        throw new UnauthorizedAccessException(
            "Invalid credentials.");
    }

    var result = await _signInManager
        .CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

    if (!result.Succeeded)
    {
        throw new UnauthorizedAccessException(
            "Invalid credentials.");
    }

    return await CreateAuthResponseAsync(user);
}

public async Task<AuthResponse> RefreshTokenAsync(
    string refreshToken)
{
    var tokenHash = HashToken(refreshToken);

    var storedToken =
        await _refreshTokenRepository
            .GetByHashAsync(tokenHash);

    if (storedToken is null ||
        !storedToken.IsActive ||
        !storedToken.User.IsActive)
    {
        throw new UnauthorizedAccessException(
            "Invalid refresh token.");
    }

    var oldToken = storedToken;

    var newRefreshToken = GenerateRefreshToken();

    var newRefreshTokenHash =
        HashToken(newRefreshToken);

    var newToken = new RefreshToken
    {
        TokenHash = newRefreshTokenHash,

        UserId = oldToken.UserId,

        CreatedAt = DateTime.UtcNow,

        ExpiresAt =
            DateTime.UtcNow.AddDays(
                RefreshTokenExpirationDays)
    };

    oldToken.RevokedAt = DateTime.UtcNow;

    oldToken.ReplacedByTokenHash =
        newRefreshTokenHash;

    await _refreshTokenRepository
        .UpdateAsync(oldToken);

    await _refreshTokenRepository
        .AddAsync(newToken);

    var (accessToken, accessTokenExpiresAt) =
        await _jwtService
            .GenerateAccessTokenAsync(oldToken.User);

    return new AuthResponse
    {
        AccessToken = accessToken,

        RefreshToken = newRefreshToken,

        AccessTokenExpiresAt =
            accessTokenExpiresAt,

        RefreshTokenExpiresAt =
            newToken.ExpiresAt,

        User = MapUser(oldToken.User)
    };
}

public async Task RevokeRefreshTokenAsync(
    string refreshToken)
{
    var tokenHash = HashToken(refreshToken);

    var storedToken =
        await _refreshTokenRepository
            .GetByHashAsync(tokenHash);

    if (storedToken is null ||
        !storedToken.IsActive)
    {
        return;
    }

    storedToken.RevokedAt = DateTime.UtcNow;

    await _refreshTokenRepository
        .UpdateAsync(storedToken);
}

private async Task<AuthResponse>
    CreateAuthResponseAsync(ApplicationUser user)
{
    var (accessToken, accessTokenExpiresAt) =
        await _jwtService
            .GenerateAccessTokenAsync(user);

    var refreshToken =
        GenerateRefreshToken();

    var refreshTokenHash =
        HashToken(refreshToken);

    var refreshTokenEntity = new RefreshToken
    {
        TokenHash = refreshTokenHash,

        UserId = user.Id,

        CreatedAt = DateTime.UtcNow,

        ExpiresAt =
            DateTime.UtcNow.AddDays(
                RefreshTokenExpirationDays)
    };

    await _refreshTokenRepository
        .AddAsync(refreshTokenEntity);

    return new AuthResponse
    {
        AccessToken = accessToken,

        RefreshToken = refreshToken,

        AccessTokenExpiresAt =
            accessTokenExpiresAt,

        RefreshTokenExpiresAt =
            refreshTokenEntity.ExpiresAt,

        User = MapUser(user)
    };
}

private static string GenerateRefreshToken()
{
    var bytes = RandomNumberGenerator
        .GetBytes(64);

    return Convert.ToBase64String(bytes);
}

private static string HashToken(string token)
{
    var bytes = SHA256.HashData(
        Encoding.UTF8.GetBytes(token));

    return Convert.ToHexString(bytes);
}

private static UserInfo MapUser(
    ApplicationUser user)
{
    return new UserInfo
    {
        Id = user.Id,

        Email = user.Email ?? string.Empty,

        FirstName = user.FirstName,

        LastName = user.LastName
    };
}

}
