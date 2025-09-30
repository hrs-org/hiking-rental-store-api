namespace HRS.API.Contracts.DTOs.Auth;

public class EmailVerificationRedirectDto
{
    public Uri RedirectUrl { get; set; } = new Uri("http://localhost:4200");
    public bool IsVerified { get; set; }
    public string Message { get; set; } = string.Empty;
}
