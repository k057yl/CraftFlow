using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Identity.Domain
{
    public sealed class User : AggregateRoot, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string FullName { get; private set; } = null!;

        private User() { }

        public static User Create(Guid tenantId, string email, string passwordHash, string fullName)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

            return new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                FullName = fullName
            };
        }
    }
}
