using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

using HRS.API.Services.Interfaces;

namespace HRS.API.Services;

public class PaymentService : IPaymentService
{
    private readonly IUserContextService _userContextService;

    public PaymentService(IUserContextService userContextService)
    {
        _userContextService = userContextService;
    }

    public async Task<Price> CreatePaymentOrder(string Productname, double amount)
    {
        StripeConfiguration.ApiKey = "";
        var user = await _userContextService.GetUserAsync();
        string ID = user.Id.ToString();
        string ordername = Productname + "-User:" + ID;
        long unitAmount = (long)(amount * 100);
        var options = new PriceCreateOptions
        {
            Currency = "SGD",
            UnitAmount = unitAmount,
            ProductData = new PriceProductDataOptions { Name = ordername }
        };
        var service = new PriceService();
        Price price = await service.CreateAsync(options);
        return price;
    }

    public async Task<string> CreatePaymentCheckOut(string Productid)
    {
        StripeConfiguration.ApiKey = "";

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            SuccessUrl = "https://example.com/success",
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
    {
        new Stripe.Checkout.SessionLineItemOptions
        {
            Price = Productid,
            Quantity = 1,
        },
    },
            Mode = "payment",
        };
        var service = new Stripe.Checkout.SessionService();
        Stripe.Checkout.Session session = await service.CreateAsync(options);
        // Console.WriteLine(session);
        return session.Url;
    }
}
