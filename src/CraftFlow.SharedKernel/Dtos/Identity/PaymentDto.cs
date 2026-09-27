namespace CraftFlow.SharedKernel.Dtos.Identity;

public record PaymentDto(
    Guid Id,
    DateTime CreatedAtUtc,
    string PlanCode,
    int DaysAdded,
    string Description
);