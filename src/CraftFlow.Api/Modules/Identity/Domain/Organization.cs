using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class Organization : AggregateRoot
{
    private readonly List<OrganizationMember> _members = new();

    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsSelfDeactivated { get; private set; }
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
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            IsSelfDeactivated = false
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
        IsActive = false;
        IsSelfDeactivated = true;
        DeactivatedAtUtc = DateTime.UtcNow;

        foreach (var member in _members)
        {
            member.Deactivate();
        }
    }

    public void Activate()
    {
        IsActive = true;
        IsSelfDeactivated = false;
        DeactivatedAtUtc = null;
        LastRetentionNoticeSentAtUtc = null;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsSelfDeactivated = false;
    }
}