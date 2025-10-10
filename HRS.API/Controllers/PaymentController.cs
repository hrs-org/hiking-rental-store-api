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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateOrder(string Productname, double amount)
    {
        var result = await _paymentService.CreatePaymentOrder(Productname, amount);
        return Ok(result);
    }

    [HttpPost("checkout")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCheckoutSession(string orderID)
    {

        var url = await _paymentService.CreatePaymentCheckOut(orderID);
        return Ok(url);
    }

}


