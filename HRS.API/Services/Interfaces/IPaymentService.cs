using Stripe;

namespace HRS.API.Services.Interfaces;

public interface IPaymentService
{
    Task<Price> CreatePaymentOrder(string Productname, double amount);
    Task<string> CreatePaymentCheckOutWithPaymentOrder(string Productid);
    Task<Stripe.Checkout.Session> CreatePaymentCheckOutWithOnlyPrice(string productName, double amount);
    Task<Stripe.Checkout.Session> CreatePaymentCheckOutInCart();
    Task<Stripe.Checkout.Session> GetSessionStatusAsync(string clientSecret);

}
