using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.RawMaterials.CreateRawMaterial;

public record CreateRawMaterialCommand(string Name, Guid UnitOfMeasureId) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.RAW_MATERIALS_MANAGE;
}