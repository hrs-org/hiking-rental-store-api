using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.User;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

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
    public async Task<ActionResult> CreateCheckoutSessionwithPrice(string Productname, double amount)
    {

        var session = await _paymentService.CreatePaymentCheckOutWithOnlyPrice(Productname, amount);
        string url = session.Url;

        return Ok(url);
    }

}


