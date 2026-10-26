using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Procurement.Suppliers.ToggleSupplierActive;

public record ToggleSupplierActiveCommand(
    Guid Id,
    TenantRole UserRole
) : IRequest<Result>;