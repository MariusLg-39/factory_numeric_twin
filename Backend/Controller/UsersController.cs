using Microsoft.AspNetCore.Mvc;
using ProjetFullstack.DTO.Users;
using ProjetFullstack.Services.Interfaces;

namespace ProjetFullstack.Controller;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // ============================================================
    // GET ALL
    // GET /api/users
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    // ============================================================
    // GET BY ID
    // GET /api/users/{id}
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    // ============================================================
    // GET BY EMAIL
    // GET /api/users/email/{email}
    // ============================================================

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await _userService.GetByEmailAsync(email);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    // ============================================================
    // CREATE
    // POST /api/users
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = result.User!.Id
            },
            result.User
        );
    }

    // ============================================================
    // UPDATE
    // PUT /api/users/{id}
    // ============================================================

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateAsync(
            id,
            request
        );

        if (!result.Success)
        {
            if (result.Error == "Utilisateur introuvable.")
            {
                return NotFound(new
                {
                    message = result.Error
                });
            }

            return BadRequest(new
            {
                message = result.Error
            });
        }

        return Ok(result.User);
    }

    // ============================================================
    // DELETE
    // DELETE /api/users/{id}
    // ============================================================

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(new
            {
                message = result.Error
            });
        }

        return NoContent();
    }
}
