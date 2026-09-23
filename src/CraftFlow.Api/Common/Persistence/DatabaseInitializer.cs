using CraftFlow.Api.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.Persistence;

public static class DatabaseInitializer
{
    public static async Task SeedSaasPlansAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();

        var planConfigs = configuration.GetSection("SaasOptions:DefaultPlans").Get<List<PlanConfigDto>>();

        if (planConfigs == null || planConfigs.Count == 0)
        {
            return;
        }

        var existingPlans = await dbContext.SubscriptionPlans.ToListAsync();

        foreach (var config in planConfigs)
        {
            var existingPlan = existingPlans.FirstOrDefault(p => p.Code == config.Code);

            if (existingPlan != null)
            {
                existingPlan.UpdateLimits(
                    config.MaxMonthlyBatches,
                    config.MaxWarehouses,
                    config.MaxChambers,
                    config.MaxUsers
                );
            }
            else
            {
                var newPlan = SubscriptionPlan.Create(
                    config.Code,
                    config.Name,
                    config.MaxMonthlyBatches,
                    config.MaxWarehouses,
                    config.MaxChambers,
                    config.MaxUsers
                );

                await dbContext.SubscriptionPlans.AddAsync(newPlan);
            }
        }

        await dbContext.SaveChangesAsync();
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