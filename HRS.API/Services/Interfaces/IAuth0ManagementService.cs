namespace HRS.API.Services.Interfaces;

public interface IAuth0ManagementService
{
    Task AssignRoleAsync(string auth0UserId, string roleName);
    Task UpdateAppMetadataAsync(string auth0UserId, object metadata);
}
