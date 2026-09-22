namespace ProjetFullstack.DTO.Users;

public class UserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

}
