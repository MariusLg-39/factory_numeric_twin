using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.Auth;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
private const string RefreshCookieName = "refreshToken";
private const string RefreshCookiePath = "/api/auth";
private readonly IAuthService _authService;
private readonly IWebHostEnvironment _environment;

public AuthController(
    IAuthService authService,
    IWebHostEnvironment environment)
{
    _authService = authService;
    _environment = environment;
}

[HttpPost("register")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Register(
    [FromBody] RegisterRequest request)
{
    try
    {
        var user = await _authService.RegisterAsync(request);
        return Ok(user);
    }
    catch (InvalidOperationException)
    {
        return BadRequest(new
        {
            message = "Unable to create the account."
        });
    }
}

[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request)
{
    try
    {
        var response = await _authService.LoginAsync(request);

        SetRefreshTokenCookie(
            response.RefreshToken,
            response.RefreshTokenExpiresAt);

        return Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Unauthorized(new
        {
            message = "Invalid credentials."
        });
    }
}

[HttpPost("refresh")]
[AllowAnonymous]
public async Task<IActionResult> Refresh()
{
    var refreshToken = Request.Cookies[RefreshCookieName];

    if (string.IsNullOrWhiteSpace(refreshToken))
    {
        DeleteRefreshTokenCookie();

        return Unauthorized(new
        {
            message = "Invalid refresh token."
        });
    }

    try
    {
        var response =
            await _authService.RefreshTokenAsync(refreshToken);

        SetRefreshTokenCookie(
            response.RefreshToken,
            response.RefreshTokenExpiresAt);

        return Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        DeleteRefreshTokenCookie();

        return Unauthorized(new
        {
            message = "Invalid refresh token."
        });
    }
}

[HttpPost("revoke")]
[Authorize]
public async Task<IActionResult> Revoke()
{
    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier);

    if (!Guid.TryParse(userIdValue, out var userId))
    {
        DeleteRefreshTokenCookie();
        return Unauthorized();
    }

    var refreshToken = Request.Cookies[RefreshCookieName];

    if (!string.IsNullOrWhiteSpace(refreshToken))
    {
        await _authService.RevokeRefreshTokenAsync(
            refreshToken,
            userId);
    }

    DeleteRefreshTokenCookie();

    return NoContent();
}

private void SetRefreshTokenCookie(
    string refreshToken,
    DateTime expiresAt)
{
    Response.Cookies.Append(
        RefreshCookieName,
        refreshToken,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(
                DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc)),
            Path = RefreshCookiePath,
            IsEssential = true
        });
}

private void DeleteRefreshTokenCookie()
{
    Response.Cookies.Delete(
        RefreshCookieName,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath
        });
}
}
