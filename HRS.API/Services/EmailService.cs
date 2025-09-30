using System.Net;
using System.Net.Mail;
using HRS.API.Models;
using HRS.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRS.API.Services;

public partial class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _baseUrl;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<EmailService>();

        _smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@hrs.com";
        _fromName = _configuration["Email:FromName"] ?? "Hiking Rental Store Team";
        _baseUrl = _configuration["Email:BaseUrl"] ?? "https://localhost:5001";
        _frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:4200";
    }

    public async Task SendVerificationEmailAsync(string email, string verificationToken, string firstName)
    {
        LogSendingVerificationEmail(_logger, email, firstName);

        var subject = "Verify Your Email - Hiking Rental Store";
        var baseUrl = _baseUrl.TrimEnd('/');
        var verificationUrl = $"{baseUrl}/api/auth/verify-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(verificationToken)}";



        var emailTemplate = new EmailTemplate
        {
            Title = $"Welcome to Hiking Rental Store, {firstName}!",
            Content = "Thank you for registering with Hiking Rental Store. To complete your registration, please verify your email address by clicking the button below:",
            ButtonText = "Verify Email Address",
            ButtonUrl = new Uri(verificationUrl),
            AdditionalInfo = "This link will expire in 24 hours.",
            FooterText = "If you didn't create an account with Hiking Rental Store, please ignore this email."
        };

        var body = GenerateEmailBody(emailTemplate);
        var result = await SendEmailAsync(email, subject, body);

        if (result)
        {
            LogVerificationEmailSent(_logger, email);
        }
        else
        {
            LogVerificationEmailFailed(_logger, email);
        }
    }

    private static string GenerateEmailBody(EmailTemplate template)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>{template.Title}</h2>
                    <p>{template.Content}</p>
                    
                    {(string.IsNullOrEmpty(template.ButtonText) ? "" : $@"
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{template.ButtonUrl}' 
                           style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                            {template.ButtonText}
                        </a>
                    </div>
                    
                    <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #7f8c8d;'>{template.ButtonUrl}</p>")}
                    
                    {(string.IsNullOrEmpty(template.AdditionalInfo) ? "" : $"<p><strong>{template.AdditionalInfo}</strong></p>")}
                    
                    <hr style='border: none; border-top: 1px solid #ecf0f1; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #7f8c8d;'>
                        {template.FooterText}
                    </p>
                </div>
            </body>
            </html>";
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort);
            client.EnableSsl = _smtpPort == 587;
            client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

            using var message = new MailMessage();
            message.From = new MailAddress(_fromEmail, _fromName);
            message.To.Add(recipient);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            await client.SendMailAsync(message);

            LogEmailSentSuccessfully(_logger, recipient);
            return true;
        }
        catch (Exception ex)
        {
            LogEmailSendFailed(_logger, recipient, ex);
            return false;
        }
    }

    // LoggerMessage methods for better performance
    [LoggerMessage(LogLevel.Information, "Sending verification email to {Email} for user {FirstName}")]
    private static partial void LogSendingVerificationEmail(ILogger logger, string email, string firstName);

    [LoggerMessage(LogLevel.Information, "Verification email sent successfully to {Email}")]
    private static partial void LogVerificationEmailSent(ILogger logger, string email);

    [LoggerMessage(LogLevel.Warning, "Failed to send verification email to {Email}")]
    private static partial void LogVerificationEmailFailed(ILogger logger, string email);

    [LoggerMessage(LogLevel.Information, "Email sent successfully to {Email}")]
    private static partial void LogEmailSentSuccessfully(ILogger logger, string email);

    [LoggerMessage(LogLevel.Error, "Failed to send email to {Email}")]
    private static partial void LogEmailSendFailed(ILogger logger, string email, Exception ex);
}

