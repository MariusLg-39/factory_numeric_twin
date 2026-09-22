using System.ComponentModel.DataAnnotations;

namespace ProjetFullstack.DTO.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
