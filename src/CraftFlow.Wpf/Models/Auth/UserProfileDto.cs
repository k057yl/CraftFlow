namespace CraftFlow.Wpf.Models.Auth;

public class UserProfileDto
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}