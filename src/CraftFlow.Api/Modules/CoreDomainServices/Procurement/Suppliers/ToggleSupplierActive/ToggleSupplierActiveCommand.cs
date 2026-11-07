using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Procurement.Suppliers.ToggleSupplierActive;

public record ToggleSupplierActiveCommand(Guid Id) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SUPPLIERS_MANAGE;
}