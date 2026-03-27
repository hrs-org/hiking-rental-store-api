namespace HRS.API.Configuration;

public class Auth0Options
{
    public string Domain { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ManagementClientId { get; set; } = string.Empty;
    public string ManagementClientSecret { get; set; } = string.Empty;
    public string OwnerRoleName { get; set; } = "Owner";
    public string CustomerRoleName { get; set; } = "Customer";
}
