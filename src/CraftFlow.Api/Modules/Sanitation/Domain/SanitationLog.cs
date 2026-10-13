using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Sanitation;

namespace CraftFlow.Api.Modules.Sanitation.Domain;

public sealed class SanitationLog : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid TaskId { get; private set; }
    public SanitationTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }

    public Guid PerformedByUserId { get; private set; }
    public Guid VerifiedByUserId { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public DateTime VerifiedAt { get; private set; }
    public int DurationMinutes { get; private set; }

    public string ChemicalsUsed { get; private set; } = null!;
    public string? Notes { get; private set; }
    public string? VerificationNotes { get; private set; }
    public bool IsPassed { get; private set; }

    private SanitationLog() { }

    public static SanitationLog CreateFromTask(SanitationTask task)
    {

        if (!task.PerformedByUserId.HasValue)
            throw new InvalidOperationException(ErrorCodes.Sanitation.PERFORMER_REQUIRED);

        if (!task.VerifiedByUserId.HasValue || !task.VerifiedAt.HasValue)
            throw new InvalidOperationException(ErrorCodes.Sanitation.VERIFIER_REQUIRED);

        if (!task.StartedAt.HasValue || !task.CompletedAt.HasValue || !task.DurationMinutes.HasValue)
            throw new InvalidOperationException(ErrorCodes.Sanitation.INVALID_DURATION);

        return new SanitationLog
        {
            Id = Guid.NewGuid(),
            TenantId = task.TenantId,
            TaskId = task.Id,
            TargetType = task.TargetType,
            TargetId = task.TargetId,
            PerformedByUserId = task.PerformedByUserId.Value,
            VerifiedByUserId = task.VerifiedByUserId.Value,
            StartedAt = task.StartedAt.Value,
            CompletedAt = task.CompletedAt.Value,
            VerifiedAt = task.VerifiedAt.Value,
            DurationMinutes = task.DurationMinutes.Value,
            ChemicalsUsed = task.ChemicalsUsed ?? string.Empty,
            Notes = task.Notes,
            VerificationNotes = task.VerificationNotes,
            IsPassed = task.Status == SanitationTaskStatus.Verified
        };
    }
}