namespace ProjetFullstack.DTO.Auth;

public class AuthResponse
{
public string AccessToken { get; set; } = string.Empty;

public string RefreshToken { get; set; } = string.Empty;

public DateTime AccessTokenExpiresAt { get; set; }

public DateTime RefreshTokenExpiresAt { get; set; }

public UserInfo User { get; set; } = new();

}

public class UserInfo
{
public Guid Id { get; set; }

public string Email { get; set; } = string.Empty;

public string FirstName { get; set; } = string.Empty;

public string LastName { get; set; } = string.Empty;

}
