namespace CraftFlow.SharedKernel.Dtos.Subscription;
public record PendingInvoiceDto(
    Guid Id,
    Guid TenantId,
    string OrganizationName,
    string PlanCode,
    int DaysRequested,
    DateTime CreatedAtUtc
);