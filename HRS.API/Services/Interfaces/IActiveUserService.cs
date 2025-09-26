using HRS.Domain.Entities;

namespace HRS.API.Services.Interfaces;

public interface IActiveUserService
{
    Task<User> GetActiveUserAsync();
}
