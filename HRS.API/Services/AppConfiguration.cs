using HRS.API.Services.Interfaces;

namespace HRS.API.Services;

public class AppConfiguration : IAppConfiguration
{
    private readonly IConfiguration _configuration;

    public AppConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
        Setup();
    }

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = string.Empty;

    private void Setup()
    {
        SmtpHost = _configuration["SmtpHost"] ?? "";
        SmtpPort = int.Parse(_configuration["SmtpPort"] ?? "0");
        SmtpUsername = _configuration["SmtpUsername"] ?? "";
        SmtpPassword = _configuration["SmtpPassword"] ?? "";
        FromEmail = _configuration["FromEmail"] ?? "";
        FrontendUrl = _configuration["FrontendUrl"] ?? "";
    }
}
