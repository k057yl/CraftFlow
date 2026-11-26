using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Subscription;
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

            var targetModule = Enum.TryParse<AppModule>(config.TargetModule, true, out var parsedModule)
                ? parsedModule
                : AppModule.NONE;

            var features = ParseFeatures(config.Features);

            if (existingPlan != null)
            {
                existingPlan.UpdateLimits(
                    targetModule,
                    features,
                    config.MaxMonthlyBatches,
                    config.MaxWarehouses,
                    config.MaxChambers,
                    config.MaxUsers,
                    config.AuditRetentionDays,
                    config.Name
                );
            }
            else
            {
                var newPlan = SubscriptionPlan.Create(
                    config.Code,
                    config.Name,
                    targetModule,
                    features,
                    config.MaxMonthlyBatches,
                    config.MaxWarehouses,
                    config.MaxChambers,
                    config.MaxUsers,
                    config.AuditRetentionDays
                );

                await dbContext.SubscriptionPlans.AddAsync(newPlan);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static PlanFeatures ParseFeatures(List<string>? featureNames)
    {
        if (featureNames == null || featureNames.Count == 0)
        {
            return PlanFeatures.NONE;
        }

        var result = PlanFeatures.NONE;
        foreach (var name in featureNames)
        {
            if (Enum.TryParse<PlanFeatures>(name, true, out var parsed))
            {
                result |= parsed;
            }
        }

        return result;
    }

    private sealed class PlanConfigDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TargetModule { get; set; } = string.Empty;
        public List<string>? Features { get; set; }
        public int MaxMonthlyBatches { get; set; }
        public int MaxWarehouses { get; set; }
        public int MaxChambers { get; set; }
        public int MaxUsers { get; set; }
        public int AuditRetentionDays { get; set; } = 7;
    }
}