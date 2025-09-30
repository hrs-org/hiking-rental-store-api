namespace HRS.API.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string verificationToken, string firstName);
    Task<bool> SendEmailAsync(string recipient, string subject, string body, bool isHtml = true);
}
