using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Sanitation;

namespace CraftFlow.Api.Modules.Sanitation.Domain;

public sealed class SanitationTask : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? RuleId { get; private set; }
    public SanitationTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public SanitationTaskStatus Status { get; private set; }

    public DateTime ScheduledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? DurationMinutes { get; private set; }

    public Guid? PerformedByUserId { get; private set; }
    public Guid? VerifiedByUserId { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    public string? ChemicalsUsed { get; private set; }
    public string? Notes { get; private set; }
    public string? VerificationNotes { get; private set; }

    private SanitationTask() { }

    public static SanitationTask Create(
        Guid tenantId,
        SanitationTargetType targetType,
        Guid targetId,
        DateTime scheduledAt,
        Guid? ruleId = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Auth.INVALID_CREDENTIALS);

        if (targetId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Sanitation.TARGET_ID_REQUIRED);

        return new SanitationTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RuleId = ruleId,
            TargetType = targetType,
            TargetId = targetId,
            Status = SanitationTaskStatus.PENDING,
            ScheduledAt = scheduledAt
        };
    }

    public void Start(Guid performerUserId, DateTime startedAt)
    {
        if (Status != SanitationTaskStatus.PENDING)
            throw new InvalidOperationException(ErrorCodes.Sanitation.INVALID_STATUS_TRANSITION);

        if (performerUserId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Sanitation.PERFORMER_REQUIRED);

        PerformedByUserId = performerUserId;
        StartedAt = startedAt;
        Status = SanitationTaskStatus.IN_PROGRESS;
    }

    public void Complete(DateTime completedAt, string chemicalsUsed, string? notes = null)
    {
        if (Status != SanitationTaskStatus.IN_PROGRESS)
            throw new InvalidOperationException(ErrorCodes.Sanitation.INVALID_STATUS_TRANSITION);

        if (!StartedAt.HasValue || completedAt < StartedAt.Value)
            throw new ArgumentException(ErrorCodes.Sanitation.INVALID_DURATION);

        if (string.IsNullOrWhiteSpace(chemicalsUsed))
            throw new ArgumentException(ErrorCodes.Sanitation.CHEMICAL_REQUIRED);

        CompletedAt = completedAt;
        DurationMinutes = (int)(completedAt - StartedAt.Value).TotalMinutes;
        ChemicalsUsed = chemicalsUsed;
        Notes = notes;
        Status = SanitationTaskStatus.COMPLETED;
    }

    public SanitationLog Verify(Guid verifierUserId, DateTime verifiedAt, bool isPassed, string? verificationNotes = null)
    {
        if (Status != SanitationTaskStatus.COMPLETED)
            throw new InvalidOperationException(ErrorCodes.Sanitation.TASK_NOT_COMPLETED);

        if (verifierUserId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Sanitation.VERIFIER_REQUIRED);

        VerifiedByUserId = verifierUserId;
        VerifiedAt = verifiedAt;
        VerificationNotes = verificationNotes;

        Status = isPassed ? SanitationTaskStatus.VERIFIED : SanitationTaskStatus.FAILED;

        return SanitationLog.CreateFromTask(this);
    }

    public void Cancel(string reason)
    {
        if (Status == SanitationTaskStatus.VERIFIED || Status == SanitationTaskStatus.COMPLETED)
            throw new InvalidOperationException(ErrorCodes.Sanitation.INVALID_STATUS_TRANSITION);

        Notes = reason;
        Status = SanitationTaskStatus.CANCELLED;
    }
}