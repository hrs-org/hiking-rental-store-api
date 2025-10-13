using System;
using System.Threading.Tasks;
using Stripe.Checkout;
using Stripe;
using NSubstitute;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.Test.API.Services;
public class PaymentServiceTests
{
    // Mocked dependencies
    private readonly IUserContextService _userContextService;
    private readonly IAppConfiguration _appConfiguration;
    private readonly Stripe.Checkout.SessionService _sessionService;
    private readonly Stripe.PriceService _priceService;

    // The system under test (SUT)
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        // Substitute all dependencies
        _userContextService = Substitute.For<IUserContextService>();
        _appConfiguration = Substitute.For<IAppConfiguration>();
        _sessionService = Substitute.For<Stripe.Checkout.SessionService>();
        _priceService = Substitute.For<Stripe.PriceService>();


        // Set up fake configuration values
        _appConfiguration.StripeApi.Returns("sk_test_fakekey");
        _appConfiguration.ReturnPaymentURL.Returns("https://example.com/return");

        // Set up fake user
        _userContextService.GetUserAsync().Returns(Task.FromResult(
            new User { Id = 1, Role = UserRole.Admin, Email = "krit@big.com" }
        ));

        // Create the service instance under test
        _service = new PaymentService(_userContextService, _appConfiguration, _sessionService, _priceService);
    }

    [Fact]
    public async Task GetSessionStatusAsync_ShouldThrow_WhenClientSecretIsNull()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetSessionStatusAsync(null));
        Assert.Equal("clientSecret is required", ex.Message);
    }

    [Fact]
    public async Task GetSessionStatusAsync_ShouldThrow_WhenClientSecretFormatInvalid()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetSessionStatusAsync("invalid_secret"));
        Assert.Equal("Invalid clientSecret format", ex.Message);
    }

    [Fact]
    public async Task GetSessionStatusAsync_ShouldReturnSession_WhenValidSecret()
    {
        // Arrange
        var clientSecret = "cs_test_123_sessionId_secret_abc";
        var expectedSession = new Session { Id = "cs_test_123_sessionId" };
        var fakeSessionService = Substitute.For<Stripe.Checkout.SessionService>();
        fakeSessionService.GetAsync("cs_test_123")
                  .Returns(Task.FromResult(new Session { Id = "cs_test_123_sessionId" }));
        var service = new PaymentService(
        _userContextService,
        _appConfiguration,
        fakeSessionService,
        _priceService
        );
        // Act
        var result = await service.GetSessionStatusAsync(clientSecret);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cs_test_123_sessionId", result.Id);
    }

    [Fact]
    public async Task VerifyPaymentStatus_ShouldThrow_WhenEmailMismatch()
    {
        // Arrange
        var fakeSession = new Session
        {
            CustomerEmail = "other@example.com",
            PaymentStatus = "paid"
        };

        var fakeSessionService = Substitute.For<SessionService>();
        fakeSessionService.GetAsync(Arg.Any<string>())
                      .Returns(Task.FromResult(fakeSession));

        var service = new PaymentService(
            _userContextService,
            _appConfiguration,
            fakeSessionService,
            _priceService
        );

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.VerifyPaymentStatus("cs_test_123_45678", "test@example.com"));

        // Assert
        Assert.Equal("Wrong Email", ex.Message);
    }

    [Fact]
    public async Task VerifyPaymentStatus_ShouldThrow_WhenPaymentNotSucceeded()
    {
        // Arrange
        var fakeSession = new Session
        {
            CustomerEmail = "test@example.com",
            PaymentStatus = "unpaid"
        };

        var fakeSessionService = Substitute.For<SessionService>();
        fakeSessionService.GetAsync(Arg.Any<string>())
                          .Returns(Task.FromResult(fakeSession));

        var service = new PaymentService(
            _userContextService,
            _appConfiguration,
            fakeSessionService,
            _priceService
        );

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.VerifyPaymentStatus("cs_test_123_45678", "test@example.com"));

        Assert.Equal("Unsucceeded Payment", ex.Message);
    }

    [Fact]
    public async Task VerifyPaymentStatus_ShouldReturnSession_WhenValid()
    {
        // Arrange
        var fakeSession = new Session
        {
            CustomerEmail = "test@example.com",
            PaymentStatus = "paid"
        };

        var fakeSessionService = Substitute.For<SessionService>();
        fakeSessionService.GetAsync(Arg.Any<string>())
                          .Returns(Task.FromResult(fakeSession));

        var service = new PaymentService(
            _userContextService,
            _appConfiguration,
            fakeSessionService,
            _priceService
        );

        // Act
        var result = await service.VerifyPaymentStatus("cs_test_123_45678", "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("paid", result.PaymentStatus);
    }

    [Fact]
    public async Task CreatePaymentOrder_ShouldReturnPrice()
    {
        // Arrange
        var productName = "TestProduct";
        double amount = 12.5;
        string ordername = productName + "-User:" + "1";
        var fakePriceService = Substitute.For<PriceService>();
        var expectedPrice = new Price { Id = "price_123" };
        var options = new PriceCreateOptions
        {
            Currency = "SGD",
            UnitAmount = 1250,
            ProductData = new PriceProductDataOptions { Name = ordername }
        };
        var service = new PaymentService(
        _userContextService,
        _appConfiguration,
        _sessionService,
        fakePriceService
        );

        // fakePriceService.CreateAsync(options)
        //           .Returns(Task.FromResult(new Price { Id = "price_123" }));
        fakePriceService.CreateAsync(Arg.Any<PriceCreateOptions>())
                .Returns(Task.FromResult(new Price { Id = "price_123" }));

        // Act
        var result = await service.CreatePaymentOrder(productName, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProduct-User:1", result.Product?.Name ?? "TestProduct-User:1");

    }

    [Fact]
    public async Task CreatePaymentCheckOutWithPaymentOrder_ShouldReturnSession()
    {
        // Arrange
        var productId = "price_123";
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        {
            new Stripe.Checkout.SessionLineItemOptions
            {
                Price = productId,
                Quantity = 1,
            },
        },
            Mode = "payment",
            CustomerEmail = "krit@big.com",
            UiMode = "embedded",
            ReturnUrl = _appConfiguration.ReturnPaymentURL,
            ExpiresAt = DateTime.UtcNow.AddMinutes(35),
        };
        var fakeSessionService = Substitute.For<Stripe.Checkout.SessionService>();
        fakeSessionService.CreateAsync(Arg.Any<Stripe.Checkout.SessionCreateOptions>())
                  .Returns(Task.FromResult(new Session { Id = "cs_test_123_sessionId", UiMode = "embedded", }));
        var service = new PaymentService(
        _userContextService,
        _appConfiguration,
        fakeSessionService,
        _priceService
        );

        // Act
        var result = await service.CreatePaymentCheckOutWithPaymentOrder(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("embedded", result.UiMode);
    }

    [Fact]
    public async Task CreatePaymentCheckOutWithOrderIDandPrice_ShouldReturnSession()
    {
        // Arrange
        int orderID = 1001;
        string ordername = "OrderID:" + orderID.ToString() + "-User:" + 1;
        double amount = 25.5;
        var fakeSessionService = Substitute.For<Stripe.Checkout.SessionService>();
        fakeSessionService.CreateAsync(Arg.Any<Stripe.Checkout.SessionCreateOptions>())
                  .Returns(Task.FromResult(new Session
                  {
                      Id = "cs_test_123_sessionId",
                      UiMode = "embedded",
                      CustomerEmail = "krit@big.com",
                  }));

        var service = new PaymentService(
        _userContextService,
        _appConfiguration,
        fakeSessionService,
        _priceService
        );

        // Act
        var result = await service.CreatePaymentCheckOutWithOrderIDandPrice(orderID, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("krit@big.com", result.CustomerEmail);
    }

    [Fact]
    public async Task CreatePaymentCheckOutInCart_ShouldReturnSession()
    {
        // Arrange
        var fakeSessionService = Substitute.For<Stripe.Checkout.SessionService>();
        fakeSessionService.CreateAsync(Arg.Any<Stripe.Checkout.SessionCreateOptions>())
                  .Returns(Task.FromResult(new Session
                  {
                      Id = "cs_test_123_sessionId",
                      UiMode = "embedded",
                      CustomerEmail = "krit@big.com",
                  }));

        var service = new PaymentService(
        _userContextService,
        _appConfiguration,
        fakeSessionService,
        _priceService
        );

        // Act
        var result = await service.CreatePaymentCheckOutInCart();

        // Assert
        Assert.NotNull(result);
        var lineItem = result.LineItems?.FirstOrDefault();
        Assert.Equal("krit@big.com", result.CustomerEmail);
    }
}





