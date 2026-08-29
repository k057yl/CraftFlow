namespace CraftFlow.Api.Common.Behaviors;

public interface IRequireQuotaValidation
{
    string QuotaMetricKey { get; }
}