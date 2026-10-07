using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class User : AggregateRoot
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;

    public string? OtpCodeHash { get; private set; }
    public DateTime? OtpExpiresAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string fullName)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            IsActive = true
        };
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

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}