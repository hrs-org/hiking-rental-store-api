using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Store;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/stores")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StoreDto>>> GetStores()
    {
        var stores = await _storeService.GetStores();
        return Ok(ApiResponse<List<StoreDto>>.OkResponse(stores));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StoreDto>> GetStoreById(int id)
    {
        var store = await _storeService.GetStoreById(id);
        if (store == null) return NotFound(ApiResponse<StoreDto>.FailResponse("Store not found"));

        return Ok(ApiResponse<StoreDto>.OkResponse(store));
    }

    [HttpGet("users/{userId:int}")]
    public async Task<ActionResult<StoreDto>> GetStoreByUserId(int userId)
    {
        var store = await _storeService.GetStoreByUserId(userId);
        if (store == null) return NotFound(ApiResponse<StoreDto>.FailResponse("Store not found"));

        return Ok(ApiResponse<StoreDto>.OkResponse(store));
    }

    [HttpPost("onboarding")]
    [Authorize(AuthenticationSchemes = "Auth0")]
    public async Task<ActionResult<bool>> CompleteStoreOnboarding([FromBody] StoreOnboardingRequestDto dto)
    {
        var res = await _storeService.CompleteStoreOnboarding(dto);
        return Ok(ApiResponse<bool>.OkResponse(res, "Store onboarding completed"));
    }
}
