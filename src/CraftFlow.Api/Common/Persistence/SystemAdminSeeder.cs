using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.Persistence;

public static class SystemAdminSeeder
{
    public static async Task SeedBigBossAsync(IServiceProvider serviceProvider)
    {
        var adminEmail = Environment.GetEnvironmentVariable(AuthConstants.AdminEnvironment.ADMIN_EMAIL)?.Trim().ToLowerInvariant();
        var adminPassword = Environment.GetEnvironmentVariable(AuthConstants.AdminEnvironment.ADMIN_PASSWORD);
        var adminName = Environment.GetEnvironmentVariable(AuthConstants.AdminEnvironment.ADMIN_NAME)?.Trim();

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existingUser = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingUser == null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

            existingUser = User.Create(
                adminEmail,
                passwordHash,
                adminName ?? AuthConstants.AdminEnvironment.DEFAULT_ADMIN_NAME
            );
            existingUser.Activate();

            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();
        }
        else if (!existingUser.IsActive)
        {
            existingUser.Activate();
            await dbContext.SaveChangesAsync();
        }

        var isAlreadySystemAdmin = await dbContext.SystemAdmins
            .AnyAsync(sa => sa.UserId == existingUser.Id);

        if (!isAlreadySystemAdmin)
        {
            var systemAdminRecord = SystemAdmin.Create(existingUser.Id);
            dbContext.SystemAdmins.Add(systemAdminRecord);
            await dbContext.SaveChangesAsync();
        }
    }
}