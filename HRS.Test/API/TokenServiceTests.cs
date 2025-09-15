using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using Microsoft.Extensions.Configuration;

// if you store UserRole here

namespace HRS.Test.API.Services;

public class TokenServiceTests
{
    private readonly ITokenService _service;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "samplekeysamplekeysamplekeysamplekey" },
            { "Jwt:Issuer", "HRSIssuer" },
            { "Jwt:Audience", "HRSAudience" }
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new TokenService(config);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt_WithExpectedClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@hrs.com",
            Role = UserRole.Admin
        };

        // Act
        var tokenString = _service.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(tokenString));

        var token = handler.ReadJwtToken(tokenString);

        var nameId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId || c.Type == ClaimTypes.NameIdentifier);
        Assert.Equal(user.Id.ToString(), nameId?.Value);

        var role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.Equal(user.Role.ToString(), role?.Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String_OfAtLeast32Bytes()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);

        var bytes = Convert.FromBase64String(refreshToken);
        Assert.True(bytes.Length >= 32, "Refresh token must be at least 32 bytes of entropy.");
    }
}
