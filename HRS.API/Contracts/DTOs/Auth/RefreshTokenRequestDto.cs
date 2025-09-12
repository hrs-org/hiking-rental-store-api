using System.ComponentModel.DataAnnotations;

namespace HRS.API.Contracts.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required] public int UserId { get; set; }

    [Required] public string RefreshToken { get; set; } = null!;
}
