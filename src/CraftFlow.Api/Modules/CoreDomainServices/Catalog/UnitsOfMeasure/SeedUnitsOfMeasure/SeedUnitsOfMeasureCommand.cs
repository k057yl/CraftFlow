using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.UnitsOfMeasure.SeedUnitsOfMeasure;

public record SeedUnitsOfMeasureCommand : IRequest<Result<int>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SETTINGS_MANAGE;
}