using FluentAssertions;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.API.Models;
using System;
using Xunit;

namespace HRS.Test.API.Services;

public class EmailBuilderServiceTests
{
    private readonly IAppConfiguration _appConfig;
    private readonly EmailBuilderService _emailBuilderService;

    public EmailBuilderServiceTests()
    {
        _appConfig = new AppConfiguration
        {
            FrontendUrl = new Uri("http://localhost:4200")
        };

        _emailBuilderService = new EmailBuilderService(_appConfig);
    }

    [Fact]
    public void BuildVerificationEmailTemplate_WithValidInputs_ReturnsCorrectTemplate()
    {
        // Arrange
        var email = "test@example.com";
        var verificationToken = "token123";
        var firstName = "John";

        // Act
        var result = _email_builder_service_Build(email, verificationToken, firstName);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Welcome to Hiking Rental Store, John!");
        result.ButtonUrl.Should().NotBeNull();
        result.ButtonUrl!.ToString().Should().Contain("http://localhost:4200/verify-email");
    }

    [Fact]
    public void GenerateEmailBody_WithButtonText_IncludesButton()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Title = "Test Title",
            Content = "Test content",
            ButtonText = "Click Here",
            ButtonUrl = new Uri("https://example.com/verify"),
            AdditionalInfo = "Additional info",
            FooterText = "Footer text"
        };

        // Act
        var result = _emailBuilderService.GenerateEmailBody(template);

        // Assert
        result.Should().Contain("Click Here");
        result.Should().Contain("https://example.com/verify");
    }

    [Fact]
    public void GenerateEmailBody_WithEmptyButtonText_DoesNotIncludeButton()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Title = "Test Title",
            Content = "Test content",
            ButtonText = "",
            ButtonUrl = new Uri("https://example.com/verify"),
            AdditionalInfo = "Additional info",
            FooterText = "Footer text"
        };

        // Act
        var result = _emailBuilderService.GenerateEmailBody(template);

        // Assert
        result.Should().NotContain("Click Here");
        result.Should().NotContain("https://example.com/verify");
    }

    [Fact]
    public void GenerateEmailBody_WithEmptyAdditionalInfo_DoesNotIncludeAdditionalInfo()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Title = "Test Title",
            Content = "Test content",
            ButtonText = "Click Here",
            ButtonUrl = new Uri("https://example.com/verify"),
            AdditionalInfo = "",
            FooterText = "Footer text"
        };

        // Act
        var result = _email_builder_service_Generate(template);

        // Assert
        result.Should().NotContain("<strong></strong>");
    }

    // helpers
    private EmailTemplate _email_builder_service_Build(string email, string token, string name)
        => _emailBuilderService.BuildVerificationEmailTemplate(email, token, name);

    private string _email_builder_service_Generate(EmailTemplate t)
        => _emailBuilderService.GenerateEmailBody(t);
}
