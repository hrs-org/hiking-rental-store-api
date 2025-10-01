using System.Net;
using System.Net.Mail;
using HRS.API.Common.Helper;
using HRS.API.Services.Interfaces;

namespace HRS.API.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IAppConfiguration _cfg;

    public EmailSenderService(ILoggerFactory loggerFactory, IAppConfiguration cfg)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<EmailSenderService>();
        _cfg = cfg;
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_cfg.SmtpHost, _cfg.SmtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_cfg.SmtpUsername, _cfg.SmtpPassword);

            using var message = new MailMessage();
            message.From = new MailAddress(_cfg.FromEmail, _cfg.FromName);
            message.To.Add(recipient);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            await client.SendMailAsync(message);

            EmailHelper.LogEmailSentSuccessfully(_logger, recipient);
            return true;
        }
        catch (Exception ex)
        {
            EmailHelper.LogEmailSendFailed(_logger, recipient, ex);
            return false;
        }
    }
}
