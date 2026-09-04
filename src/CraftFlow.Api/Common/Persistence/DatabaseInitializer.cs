using CraftFlow.Api.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.Persistence;

public static class DatabaseInitializer
{
    public static async Task SeedSaasPlansAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();

        if (await dbContext.SubscriptionPlans.AnyAsync())
        {
            return;
        }

        var planConfigs = configuration.GetSection("SaasOptions:DefaultPlans").Get<List<PlanConfigDto>>();

        if (planConfigs != null && planConfigs.Count > 0)
        {
            var plans = planConfigs.Select(c => new SubscriptionPlan(
                Guid.NewGuid(),
                c.Code,
                c.Name,
                c.MaxMonthlyBatches,
                c.MaxWarehouses,
                c.MaxChambers,
                c.MaxUsers
            )).ToList();

            await dbContext.SubscriptionPlans.AddRangeAsync(plans);
            await dbContext.SaveChangesAsync();
        }
    }

    private record PlanConfigDto(
        string Code,
        string Name,
        int MaxMonthlyBatches,
        int MaxWarehouses,
        int MaxChambers,
        int MaxUsers
    );
}