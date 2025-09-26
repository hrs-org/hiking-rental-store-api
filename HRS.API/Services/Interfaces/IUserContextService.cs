using HRS.Domain.Entities;

namespace HRS.API.Services.Interfaces;

public interface IUserContextService
{
    Task<User> GetUserAsync();

    Task<int> GetUserIdAsync();
}
