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

    public bool IsAdmin { get; private set; }
    public bool IsActive { get; private set; }

    private User() { }

    public static User Create(Guid tenantId, string email, string passwordHash, string fullName)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            IsAdmin = false,
            IsActive = false
        };
    }

    public static User CreateSystemAdmin(string email, string fullName, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            IsAdmin = true,
            IsActive = true
        };
    }

    public void Activate()
    {
        IsActive = true;
        ClearOtpCode();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

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