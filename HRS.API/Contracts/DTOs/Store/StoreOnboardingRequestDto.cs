namespace HRS.API.Contracts.DTOs.Store;

public class StoreOnboardingRequestDto
{
    public required string Auth0UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
}
