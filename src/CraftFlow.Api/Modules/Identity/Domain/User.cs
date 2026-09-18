using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class User : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;

    public string? OtpCodeHash { get; private set; }
    public DateTime? OtpExpiresAtUtc { get; private set; }

    public bool IsActive { get; private set; }
    public TenantRole Role { get; private set; }

    private User() { }

    public static User Create(Guid tenantId, string email, string passwordHash, string fullName, TenantRole role = TenantRole.Owner)
    {
        if (role == TenantRole.SuperAdmin && tenantId != Guid.Empty)
        {
            role = TenantRole.Owner;
        }

        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            IsActive = false,
            Role = role
        };
    }

    public void ChangeRole(TenantRole newRole)
    {
        if (newRole == TenantRole.SuperAdmin && TenantId != Guid.Empty)
        {
            return;
        }

        Role = newRole;
    }

    public void Activate()
    {
        IsActive = true;
        ClearOtpCode();
    }

    public void Deactivate() => IsActive = false;

    public void SetOtpCode(string codeHash, DateTime expiresAtUtc)
    {
        OtpCodeHash = codeHash;
        OtpExpiresAtUtc = expiresAtUtc;
    }

    public void ClearOtpCode()
    {
        OtpCodeHash = null;
        OtpExpiresAtUtc = null;
    }
}