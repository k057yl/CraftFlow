namespace CraftFlow.Api.Common.Behaviors;

public interface IRequireQuotaValidation
{
    QuotaType QuotaType { get; }
}