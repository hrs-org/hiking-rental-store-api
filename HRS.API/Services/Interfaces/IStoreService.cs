using HRS.API.Contracts.DTOs.Store;

namespace HRS.API.Services.Interfaces;

public interface IStoreService
{
    Task<bool> CompleteStoreOnboarding(StoreOnboardingRequestDto dto);
    Task<StoreDto?> GetStoreById(int id);
    Task<StoreDto?> GetStoreByUserId(int userId);
    Task<List<StoreDto>> GetStores();
}
