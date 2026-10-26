namespace CraftFlow.SharedKernel.Dtos.Supplier;

public record CreateSupplierRequest(
    string Name,
    string? LegalName,
    string? TaxId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes
);