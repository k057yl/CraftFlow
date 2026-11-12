using CraftFlow.SharedKernel.Enums;

namespace CraftFlow.SharedKernel.Dtos.Subscription;

public record MySubscriptionOverviewDto(
    string StatusBadge,
    string ActivePlansSummary,
    DateTime? NearestExpirationUtc,
    AppModule ActiveModules,
    List<SubscriptionPaymentDto> Payments
);