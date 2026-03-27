namespace HRS.API.Contracts.DTOs.User;

public class AssignCustomerRoleDto
{
    public required string Auth0UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
