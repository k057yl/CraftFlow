using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.UnitsOfMeasure.SeedUnitsOfMeasure;

public record SeedUnitsOfMeasureCommand : IRequest<Result<int>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SETTINGS_MANAGE;
}