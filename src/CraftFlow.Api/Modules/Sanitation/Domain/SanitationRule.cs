using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Sanitation;

namespace CraftFlow.Api.Modules.Sanitation.Domain;

public sealed class SanitationRule : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public SanitationTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public SanitationTriggerType TriggerType { get; private set; }
    public int? IntervalDays { get; private set; }
    public int? MaxBatchCount { get; private set; }
    public DateTime? NextScheduledAt { get; private set; }
    public string RequiredChemicals { get; private set; } = null!;

    private SanitationRule() { }

    public static SanitationRule Create(
        Guid tenantId,
        SanitationTargetType targetType,
        Guid targetId,
        SanitationTriggerType triggerType,
        string requiredChemicals,
        int? intervalDays = null,
        int? maxBatchCount = null,
        DateTime? nextScheduledAt = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Auth.INVALID_CREDENTIALS);

        if (targetId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Sanitation.TARGET_ID_REQUIRED);

        if (string.IsNullOrWhiteSpace(requiredChemicals))
            throw new ArgumentException(ErrorCodes.Sanitation.CHEMICAL_REQUIRED);

        ValidateTriggerConfig(triggerType, intervalDays, maxBatchCount, nextScheduledAt);

        return new SanitationRule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TargetType = targetType,
            TargetId = targetId,
            TriggerType = triggerType,
            RequiredChemicals = requiredChemicals,
            IntervalDays = intervalDays,
            MaxBatchCount = maxBatchCount,
            NextScheduledAt = nextScheduledAt
        };
    }

    private static void ValidateTriggerConfig(
        SanitationTriggerType triggerType,
        int? intervalDays,
        int? maxBatchCount,
        DateTime? nextScheduledAt)
    {
        switch (triggerType)
        {
            case SanitationTriggerType.INTERVAL_DAYS:
                if (!intervalDays.HasValue || intervalDays.Value <= 0)
                    throw new ArgumentException(ErrorCodes.Sanitation.INVALID_TRIGGER_CONFIG);
                break;

            case SanitationTriggerType.BATCH_COUNT:
                if (!maxBatchCount.HasValue || maxBatchCount.Value <= 0)
                    throw new ArgumentException(ErrorCodes.Sanitation.INVALID_TRIGGER_CONFIG);
                break;

            case SanitationTriggerType.BOTH:
                if ((!intervalDays.HasValue || intervalDays.Value <= 0) && (!maxBatchCount.HasValue || maxBatchCount.Value <= 0))
                    throw new ArgumentException(ErrorCodes.Sanitation.INVALID_TRIGGER_CONFIG);
                break;

            case SanitationTriggerType.SCHEDULED_DATE:
                if (!nextScheduledAt.HasValue)
                    throw new ArgumentException(ErrorCodes.Sanitation.INVALID_TRIGGER_CONFIG);
                break;

            case SanitationTriggerType.UPON_EMPTY:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(triggerType));
        }
    }
}