namespace CraftFlow.SharedKernel.Dtos.Supplier;
public record SupplierDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes
);