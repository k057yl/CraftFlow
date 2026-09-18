using CraftFlow.Api.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using static CraftFlow.SharedKernel.Constants.AuthConstants;

namespace CraftFlow.Api.Common.Persistence;

public static class SystemAdminSeeder
{
    public static async Task SeedBigBossAsync(IServiceProvider serviceProvider)
    {
        var adminEmail = Environment.GetEnvironmentVariable(ADMIN_CONFIG_KEYS.ADMIN_EMAIL_KEY)?.Trim().ToLowerInvariant();
        var adminPassword = Environment.GetEnvironmentVariable(ADMIN_CONFIG_KEYS.ADMIN_PASSWORD_KEY);
        var adminName = Environment.GetEnvironmentVariable(ADMIN_CONFIG_KEYS.ADMIN_NAME_KEY)?.Trim();

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existingAdmin = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin == null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

            var bigBoss = User.Create(Guid.Empty, adminEmail, passwordHash, adminName ?? "Big Boss", TenantRole.SuperAdmin);
            bigBoss.Activate();

            dbContext.Users.Add(bigBoss);
            await dbContext.SaveChangesAsync();
        }
        else if (!existingAdmin.IsActive)
        {
            existingAdmin.Activate();
            await dbContext.SaveChangesAsync();
        }
    }
}