using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Subscription;

namespace CraftFlow.SharedKernel.Dtos.Subscription;

public record MySubscriptionOverviewDto(
    string StatusBadge,
    string ActivePlansSummary,
    DateTime? NearestExpirationUtc,
    AppModule ActiveModules,
    PlanFeatures EffectiveFeatures,
    List<ModuleSubscriptionStatusDto> Modules,
    List<SubscriptionPaymentDto> Payments
);