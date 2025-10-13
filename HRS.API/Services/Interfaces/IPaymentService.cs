using Stripe;

namespace HRS.API.Services.Interfaces;

public interface IPaymentService
{
    Task<Price> CreatePaymentOrder(string Productname, double amount);
    Task<Stripe.Checkout.Session> CreatePaymentCheckOutWithPaymentOrder(string Productid);
    Task<Stripe.Checkout.Session> CreatePaymentCheckOutWithOrderIDandPrice(int orderID, double amount);
    Task<Stripe.Checkout.Session> CreatePaymentCheckOutInCart();
    Task<Stripe.Checkout.Session> GetSessionStatusAsync(string clientSecret);
    Task<Stripe.Checkout.Session> VerifyPaymentStatus(string clientSecret, string email);
}
