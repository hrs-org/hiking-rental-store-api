using System.ComponentModel.DataAnnotations;

namespace HRS.API.Contracts.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required] public string RefreshToken { get; set; } = null!;
}
