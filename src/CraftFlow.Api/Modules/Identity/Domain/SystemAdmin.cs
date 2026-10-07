using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain;

public sealed class SystemAdmin : Entity
{
    public Guid UserId { get; private set; }
    public DateTime GrantedAtUtc { get; private set; }

    private SystemAdmin() { }

    public static SystemAdmin Create(Guid userId)
    {
        return new SystemAdmin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GrantedAtUtc = DateTime.UtcNow
        };
    }
}