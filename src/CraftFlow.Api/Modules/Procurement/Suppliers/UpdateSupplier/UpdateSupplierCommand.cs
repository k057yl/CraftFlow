using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Procurement.Suppliers.UpdateSupplier;

public record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    TenantRole UserRole
) : IRequest<Result>;