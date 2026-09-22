using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.Auth;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        try
        {
            var response =
                await _authService.RegisterAsync(request);

            return Ok(response);
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
            var response =
                await _authService.LoginAsync(request);

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
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response =
                await _authService
                    .RefreshTokenAsync(
                        request.RefreshToken);

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new
            {
                message = "Invalid refresh token."
            });
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(
        [FromBody] RefreshTokenRequest request)
    {
        await _authService
            .RevokeRefreshTokenAsync(
                request.RefreshToken);

        return NoContent();
    }
}
