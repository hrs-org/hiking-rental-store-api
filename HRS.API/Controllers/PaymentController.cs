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
[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{

    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("order")]
    public async Task<ActionResult> CreateOrder(string Productname, double amount)
    {
        var result = await _paymentService.CreatePaymentOrder(Productname, amount);
        return Ok(result);
    }

    [HttpPost("checkoutOrder")]
    public async Task<ActionResult> CreateCheckoutSessionwithOrder(string orderID)
    {

        var url = await _paymentService.CreatePaymentCheckOutWithPaymentOrder(orderID);
        return Ok(url);
    }

    [HttpPost("checkoutPrice")]
    public async Task<ActionResult> CreateCheckoutSessionwithPrice([FromBody] CheckoutPriceRequest request)
    {

        var session = await _paymentService.CreatePaymentCheckOutWithOnlyPrice(request.Productname, request.Amount);
        // string url = session.Url;

        return Ok(ApiResponse<string>.OkResponse(session.ClientSecret));
    }

}


