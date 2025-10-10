using Stripe;

namespace HRS.API.Services.Interfaces;

public interface IPaymentService
{
    Task<Price> CreatePaymentOrder(string Productname, double amount);
    Task<string> CreatePaymentCheckOutWithPaymentOrder(string Productid);
    Task<Stripe.Checkout.Session> CreatePaymentCheckOutWithOnlyPrice(string productName, double amount);

}
