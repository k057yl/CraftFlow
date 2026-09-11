namespace CraftFlow.Wpf.Models.Auth;

public record OrganizationAdminDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAtUtc,
    string SubscriptionStatus,
    DateTime? SubscriptionExpiresAtUtc
)
{
    public string AccessText => IsActive ? "Разрешен" : "Заблокирован";
}