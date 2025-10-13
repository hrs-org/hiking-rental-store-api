using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

using HRS.API.Services.Interfaces;
using System.Security.Cryptography;  // Remove if not use mockup value

namespace HRS.API.Services;

public class PaymentService : IPaymentService
{
    private readonly IUserContextService _userContextService;
    private readonly IAppConfiguration _appConfiguration;
    private readonly IPaymentService _paymentService;

    public PaymentService(IUserContextService userContextService, IAppConfiguration appConfiguration, IPaymentService paymentService)
    {
        _userContextService = userContextService;
        _appConfiguration = appConfiguration;
        _paymentService = paymentService;
    }

    public async Task<Price> CreatePaymentOrder(string Productname, double amount)
    {
        StripeConfiguration.ApiKey = _appConfiguration.StripeApi;
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

    public async Task<Stripe.Checkout.Session> CreatePaymentCheckOutWithPaymentOrder(string Productid)
    {
        StripeConfiguration.ApiKey = _appConfiguration.StripeApi;
        var user = await _userContextService.GetUserAsync();
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        {
            new Stripe.Checkout.SessionLineItemOptions
            {
                Price = Productid,
                Quantity = 1,
            },
        },
            Mode = "payment",
            CustomerEmail = user.Email,
            UiMode = "embedded",
            ReturnUrl = _appConfiguration.ReturnPaymentURL,
            ExpiresAt = DateTime.UtcNow.AddMinutes(35),
        };
        var service = new Stripe.Checkout.SessionService();
        Stripe.Checkout.Session session = await service.CreateAsync(options);
        return session;
    }

    public async Task<Stripe.Checkout.Session> CreatePaymentCheckOutWithOrderIDandPrice(int orderID, double amount)
    {
        StripeConfiguration.ApiKey = _appConfiguration.StripeApi;
        var user = await _userContextService.GetUserAsync();
        string ID = user.Id.ToString();
        string ordername = "OrderID:" + orderID.ToString() + "-User:" + ID;

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        {
            new Stripe.Checkout.SessionLineItemOptions
            {
                PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100),
                    Currency = "SGD",
                    ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                    {
                        Name = ordername,
                    },
                },
                Quantity = 1,
            },
        },
            Mode = "payment",
            CustomerEmail = user.Email,
            UiMode = "embedded",
            ReturnUrl = _appConfiguration.ReturnPaymentURL,
            ExpiresAt = DateTime.UtcNow.AddMinutes(35),
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);
        return session;
    }

    public async Task<Stripe.Checkout.Session> CreatePaymentCheckOutInCart()
    {
        StripeConfiguration.ApiKey = _appConfiguration.StripeApi;
        var user = await _userContextService.GetUserAsync();
        string ID = user.Id.ToString();
        int orderID = RandomNumberGenerator.GetInt32(1000000, 9999999);       // MockUP
        double orderPrice = RandomNumberGenerator.GetInt32(100);   // MockUP
        string ordername = "OrderID:" + orderID.ToString() + "-User:" + ID;

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        {
            new Stripe.Checkout.SessionLineItemOptions
            {
                PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(orderPrice * 100),
                    Currency = "SGD",
                    ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                    {
                        Name = ordername,
                    },
                },
                Quantity = 1,
            },
        },
            Mode = "payment",
            CustomerEmail = user.Email,
            UiMode = "embedded",
            ReturnUrl = _appConfiguration.ReturnPaymentURL,
            ExpiresAt = DateTime.UtcNow.AddMinutes(35),
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);

        return session;
    }
    public async Task<Stripe.Checkout.Session> GetSessionStatusAsync(string clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentException("clientSecret is required");

        // client_secret format: cs_test_xxx_sessionId_secret_yyy

        var parts = clientSecret.Split('_');
        if (parts.Length < 4)
            throw new ArgumentException("Invalid clientSecret format");

        // sessionId = cs_test_xxx_sessionId
        string sessionId = string.Join("_", parts[0], parts[1], parts[2]);

        var service = new Stripe.Checkout.SessionService();
        var session = await service.GetAsync(sessionId);
        return session;

    }

    public async Task<Stripe.Checkout.Session> VerifyPaymentStatus(string clientSecret, string email)
    {
        var session = await _paymentService.GetSessionStatusAsync(clientSecret);
        if (session.CustomerEmail != email) throw new ArgumentException("Wrong Email");
        var status = session.PaymentStatus;
        if (status != "paid") throw new ArgumentException("Unsucceeded Payment");  //'succeeded', ""unpaid""
        return session;
    }
}
