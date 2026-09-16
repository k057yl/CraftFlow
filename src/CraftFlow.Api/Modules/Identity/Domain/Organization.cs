using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class Organization : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsSelfDeactivated { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }
    public DateTime? LastRetentionNoticeSentAtUtc { get; private set; }

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

    public void DeactivateByOwner()
    {
        IsActive = false;
        IsSelfDeactivated = true;
        DeactivatedAtUtc = DateTime.UtcNow;
    }

    public void DeactivateByAdmin()
    {
        IsActive = false;
        IsSelfDeactivated = false;
    }

    public void Deactivate() => DeactivateByAdmin();

    public void Activate()
    {
        IsActive = true;
        IsSelfDeactivated = false;
        DeactivatedAtUtc = null;
        LastRetentionNoticeSentAtUtc = null;
    }

    public void RecordRetentionNoticeSent()
    {
        LastRetentionNoticeSentAtUtc = DateTime.UtcNow;
    }
}