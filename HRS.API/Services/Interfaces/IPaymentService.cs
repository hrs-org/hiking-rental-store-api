using Stripe;

namespace HRS.API.Services.Interfaces;

public interface IPaymentService
{
    Task<Price> CreatePaymentOrder(string Productname, double amount);
    Task<string> CreatePaymentCheckOut(string Productid);

}
