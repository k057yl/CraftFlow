using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class User : AggregateRoot
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;

    public string? OtpCodeHash { get; private set; }
    public DateTime? OtpExpiresAtUtc { get; private set; }

    public UserStatus Status { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string fullName, bool requireOtp = false)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Status = requireOtp ? UserStatus.PendingActivation : UserStatus.Active
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

    public void Activate() => Status = UserStatus.Active;
    public void Deactivate() => Status = UserStatus.Blocked;
    public void Block() => Status = UserStatus.Blocked;
}