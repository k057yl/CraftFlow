using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class OrganizationMember : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }

    public TenantRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    public User User { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;

    private OrganizationMember() { }

    public static OrganizationMember Create(Guid organizationId, Guid userId, TenantRole role)
    {
        return new OrganizationMember
        {
            Id = Guid.NewGuid(),
            TenantId = organizationId,
            UserId = userId,
            Role = role,
            IsActive = true,
            JoinedAtUtc = DateTime.UtcNow
        };
    }

    public void ChangeRole(TenantRole newRole)
    {
        Role = newRole;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}