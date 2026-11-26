using CraftFlow.SharedKernel.Enums.Subscription;

namespace CraftFlow.Api.Core.Behaviors;

public interface IRequirePlanFeature
{
    PlanFeatures RequiredFeature { get; }
}