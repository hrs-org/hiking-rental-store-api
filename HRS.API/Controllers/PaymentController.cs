using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

public class CheckoutPriceRequest
{
    public string Productname { get; set; } = string.Empty;
    public double Amount { get; set; }
}

public class VerifyCheckout
{
    public string clientSecret { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
}
[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{

    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // For Create Product in Stripe
    [HttpPost("order")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateOrder(string Productname, double amount)
    {
        var result = await _paymentService.CreatePaymentOrder(Productname, amount);
        return Ok(result);
    }

    // For Checkout from Porduct in Stripe
    [HttpPost("checkoutOrder")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCheckoutSessionwithOrder(string orderID)
    {

        var url = await _paymentService.CreatePaymentCheckOutWithPaymentOrder(orderID);
        return Ok(url);
    }

    // For Checkout with seting price and Ordername
    [HttpPost("checkoutPrice")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCheckoutSessionwithPrice([FromBody] CheckoutPriceRequest request)
    {
        var session = await _paymentService.CreatePaymentCheckOutWithOnlyPrice(request.Productname, request.Amount);
        return Ok(ApiResponse<string>.OkResponse(session.ClientSecret));
    }

    [HttpGet("checkoutCart")]
    public async Task<ActionResult> CreateCheckoutSessionfromCart()
    {
        var session = await _paymentService.CreatePaymentCheckOutInCart();
        return Ok(ApiResponse<string>.OkResponse(session.ClientSecret));
    }

    [HttpPost("VerifyCheckout")]
    public async Task<ActionResult> VerifyCheckOutSession([FromBody] VerifyCheckout payload)
    {
        var session = await _paymentService.GetSessionStatusAsync(payload.clientSecret);
        if (session.CustomerEmail != payload.email) throw new ArgumentException("Wrong Email");
        var status = session.PaymentStatus;
        if (status != "paid")throw new ArgumentException("Unsucceeded Payment");  //'succeeded', ""unpaid""
        return Ok(ApiResponse<Stripe.Checkout.Session>.OkResponse(session));
    }

}


