using AutoMapper;
using HRS.API.Configuration;
using HRS.API.Contracts.DTOs.Store;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace HRS.API.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuth0ManagementService _auth0ManagementService;
    private readonly IMapper _mapper;
    private readonly Auth0Options _auth0Options;

    public StoreService(
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        IAuth0ManagementService auth0ManagementService,
        IMapper mapper,
        IOptions<Auth0Options> auth0Options)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _auth0ManagementService = auth0ManagementService;
        _mapper = mapper;
        _auth0Options = auth0Options.Value;
    }

    public async Task<bool> CompleteStoreOnboarding(StoreOnboardingRequestDto dto)
    {
        await using var tx = await _userRepository.BeginTransactionAsync();

        try
        {
            var user = await _userRepository.GetByAuth0UserIdAsync(dto.Auth0UserId) ??
                       await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
            {
                user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Auth0UserId = dto.Auth0UserId,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    IsVerified = true,
                    Role = UserRole.Manager,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            else
            {
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.Auth0UserId = dto.Auth0UserId;
                user.IsVerified = true;
                user.Role = UserRole.Manager;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            var existingStore = await _storeRepository.GetByUserIdAsync(user.Id);
            if (existingStore != null)
                throw new InvalidOperationException("Store onboarding is already completed for this user.");

            var store = new Store
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                UserId = user.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _storeRepository.AddAsync(store);
            await _storeRepository.SaveChangesAsync();

            await _auth0ManagementService.AssignRoleAsync(dto.Auth0UserId, _auth0Options.OwnerRoleName);
            await _auth0ManagementService.UpdateAppMetadataAsync(dto.Auth0UserId, new { store_id = store.Id });

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<StoreDto?> GetStoreById(int id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        return store == null ? null : _mapper.Map<StoreDto>(store);
    }

    public async Task<StoreDto?> GetStoreByUserId(int userId)
    {
        var store = await _storeRepository.GetByUserIdAsync(userId);
        return store == null ? null : _mapper.Map<StoreDto>(store);
    }

    public async Task<List<StoreDto>> GetStores()
    {
        var stores = await _storeRepository.GetActiveStoresAsync();
        return _mapper.Map<List<StoreDto>>(stores);
    }
}
