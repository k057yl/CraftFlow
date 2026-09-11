namespace CraftFlow.Api.Modules.Identity.GetOrganizations;

public record OrganizationAdminDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAtUtc,
    string SubscriptionStatus,
    DateTime? SubscriptionExpiresAtUtc
);