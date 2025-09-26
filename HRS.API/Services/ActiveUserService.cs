using System.Security.Claims;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;

namespace HRS.API.Services;

public class ActiveUserService : IActiveUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public ActiveUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public async Task<User> GetActiveUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException("User is not authenticated");

        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var userIdAvailable = int.TryParse(id, out var userId);

        if (!userIdAvailable) throw new UnauthorizedAccessException("User ID claim not found");

        var user = await _userRepository.GetByIdAsync(userId);

        return user ?? throw new UnauthorizedAccessException("User not found");
    }
}
