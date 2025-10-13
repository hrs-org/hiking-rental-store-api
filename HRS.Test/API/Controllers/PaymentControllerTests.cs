using System.Threading.Tasks;
using HRS.API.Controllers;
using HRS.API.Contracts.DTOs;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Stripe.Checkout;
using Xunit;

namespace HRS.Test.API.Controllers;

public class PaymentControllerTests
{
    private readonly IPaymentService _paymentService;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _paymentService = Substitute.For<IPaymentService>();
        _controller = new PaymentController(_paymentService);
    }

    [Fact]
    public async Task CreateOrder_ReturnsOk_WithPrice()
    {
        // Arrange
        var productName = "TestProduct";
        double amount = 50;
        var fakePrice = new Stripe.Price { Id = "price_123" };
        _paymentService.CreatePaymentOrder(productName, amount)
                       .Returns(Task.FromResult(fakePrice));

        // Act
        var result = await _controller.CreateOrder(productName, amount);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fakePrice, okResult.Value);
    }

    [Fact]
    public async Task CreateCheckoutSessionwithOrder_ReturnsOk_WithSession()
    {
        // Arrange
        var orderID = "order_123";
        var fakeSession = new Session { Id = "cs_test_123", ClientSecret = "secret_123" };
        _paymentService.CreatePaymentCheckOutWithPaymentOrder(orderID)
                       .Returns(Task.FromResult(fakeSession));

        // Act
        var result = await _controller.CreateCheckoutSessionwithOrder(orderID);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fakeSession, okResult.Value);
    }

    [Fact]
    public async Task CreateCheckoutSessionwithPrice_ReturnsOk_WithClientSecret()
    {
        // Arrange
        var request = new CheckoutPriceRequest { orderID = 1, Amount = 100 };
        var fakeSession = new Session { ClientSecret = "secret_456" };
        _paymentService.CreatePaymentCheckOutWithOrderIDandPrice(request.orderID, request.Amount)
                       .Returns(Task.FromResult(fakeSession));

        // Act
        var result = await _controller.CreateCheckoutSessionwithPrice(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
        Assert.Equal("secret_456", response.Data);
    }

    [Fact]
    public async Task CreateCheckoutSessionfromCart_ReturnsOk_WithClientSecret()
    {
        // Arrange
        var fakeSession = new Session { ClientSecret = "secret_cart" };
        _paymentService.CreatePaymentCheckOutInCart()
                       .Returns(Task.FromResult(fakeSession));

        // Act
        var result = await _controller.CreateCheckoutSessionfromCart();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
        Assert.Equal("secret_cart", response.Data);
    }

    [Fact]
    public async Task VerifyCheckOutSession_ReturnsOk_WithSession()
    {
        // Arrange
        var payload = new VerifyCheckout { clientSecret = "secret_xyz", email = "test@example.com" };
        var fakeSession = new Session { Id = "cs_abc", CustomerEmail = "test@example.com" };
        _paymentService.VerifyPaymentStatus(payload.clientSecret, payload.email)
                       .Returns(Task.FromResult(fakeSession));

        // Act
        var result = await _controller.VerifyCheckOutSession(payload);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Session>>(okResult.Value);
        Assert.Equal(fakeSession, response.Data);
    }
}

