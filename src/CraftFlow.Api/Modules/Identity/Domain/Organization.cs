using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class Organization : AggregateRoot
{
    private readonly List<OrganizationMember> _members = new();

    public string Name { get; private set; } = null!;
    public OrganizationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }
    public DateTime? LastRetentionNoticeSentAtUtc { get; private set; }

    public IReadOnlyCollection<OrganizationMember> Members => _members.AsReadOnly();

    private Organization() { }

    public static Organization Create(string name)
    {
        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Status = OrganizationStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public OrganizationMember AddMember(Guid userId, TenantRole role)
    {
        var existing = _members.FirstOrDefault(m => m.UserId == userId);
        if (existing != null)
        {
            existing.Activate();
            existing.ChangeRole(role);
            return existing;
        }

        var member = OrganizationMember.Create(Id, userId, role);
        _members.Add(member);
        return member;
    }

    public void RecordRetentionNoticeSent()
    {
        LastRetentionNoticeSentAtUtc = DateTime.UtcNow;
    }

    public void DeactivateByOwner()
    {
        Status = OrganizationStatus.DeactivatedByOwner;
        DeactivatedAtUtc = DateTime.UtcNow;

        foreach (var member in _members)
        {
            member.Deactivate();
        }
    }

    public void Activate()
    {
        Status = OrganizationStatus.Active;
        DeactivatedAtUtc = null;
        LastRetentionNoticeSentAtUtc = null;
    }

    public void Deactivate()
    {
        Status = OrganizationStatus.Suspended;
    }
}