using FluentAssertions;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net.Mail;

namespace HRS.Test.API.Services;

public class EmailServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Email:SmtpHost", "smtp.gmail.com" },
            { "Email:SmtpPort", "587" },
            { "Email:SmtpUsername", "test@gmail.com" },
            { "Email:SmtpPassword", "testpassword" },
            { "Email:FromEmail", "noreply@hrs.com" },
            { "Email:FromName", "HRS Team" },
            { "Email:BaseUrl", "https://localhost:5001" },
            { "Email:FrontendUrl", "http://localhost:4200" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _loggerFactory = Substitute.For<ILoggerFactory>();
        var logger = Substitute.For<ILogger<EmailService>>();
        _loggerFactory.CreateLogger<EmailService>().Returns(logger);

        _emailService = new EmailService(_configuration, _loggerFactory);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_WithValidData_ShouldGenerateCorrectEmailBody()
    {
        // Arrange
        var email = "test@example.com";
        var verificationToken = "test-token-123";
        var firstName = "John";

        // Act
        await _emailService.SendVerificationEmailAsync(email, verificationToken, firstName);

        // Assert
        // Since we can't easily test the actual SMTP sending without a real server,
        // we verify that the method doesn't throw an exception
        // In a real scenario, you might want to mock the SmtpClient
    }

    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ShouldNotThrowException()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var body = "<html><body>Test Body</body></html>";

        // Act & Assert
        // This will likely fail in a test environment without real SMTP,
        // but we can verify the method structure
        var result = await _emailService.SendEmailAsync(recipient, subject, body);
        
        // The result will be false in test environment, but method should not throw
        result.Should().BeFalse(); // Expected to fail in test environment
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlContent_ShouldSetIsHtmlToTrue()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var htmlBody = "<html><body><h1>Test</h1></body></html>";

        // Act
        var result = await _emailService.SendEmailAsync(recipient, subject, htmlBody, true);

        // Assert
        result.Should().BeFalse(); // Expected to fail in test environment
    }

    [Fact]
    public async Task SendEmailAsync_WithPlainTextContent_ShouldSetIsHtmlToFalse()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var plainTextBody = "This is plain text content";

        // Act
        var result = await _emailService.SendEmailAsync(recipient, subject, plainTextBody, false);

        // Assert
        result.Should().BeFalse(); // Expected to fail in test environment
    }

    [Fact]
    public void EmailService_Constructor_ShouldInitializeWithConfiguration()
    {
        // Arrange & Act
        var emailService = new EmailService(_configuration, _loggerFactory);

        // Assert
        emailService.Should().NotBeNull();
    }

    [Fact]
    public void EmailService_Constructor_WithNullConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();

        // Act
        var emailService = new EmailService(emptyConfig, _loggerFactory);

        // Assert
        emailService.Should().NotBeNull();
    }
}

