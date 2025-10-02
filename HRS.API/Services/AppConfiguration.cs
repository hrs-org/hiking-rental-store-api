using HRS.API.Services.Interfaces;
using System;

namespace HRS.API.Services;

public class AppConfiguration : IAppConfiguration
{
    public AppConfiguration()
    {

    }

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public System.Uri BaseUrl { get; set; } = new Uri("http://localhost");
    public Uri FrontendUrl { get; set; } = new Uri("http://localhost");
}
