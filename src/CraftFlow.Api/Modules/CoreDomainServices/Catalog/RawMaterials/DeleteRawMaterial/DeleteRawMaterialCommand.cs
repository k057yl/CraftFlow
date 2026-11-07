using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.RawMaterials.DeleteRawMaterial;

public record DeleteRawMaterialCommand(Guid Id) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.RAW_MATERIALS_MANAGE;
}