using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Core.Persistence;

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

            var baseModules = Enum.TryParse<AppModule>(config.BaseModules, true, out var parsedModules)
                ? parsedModules
                : AppModule.NONE;

            if (existingPlan != null)
            {
                existingPlan.UpdateLimits(
                    baseModules,
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
                    baseModules,
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
        string BaseModules,
        int MaxMonthlyBatches,
        int MaxWarehouses,
        int MaxChambers,
        int MaxUsers
    );
}