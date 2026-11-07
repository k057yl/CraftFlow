using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.UnitsOfMeasure
{
    public record GetUnitsOfMeasureQuery : IRequest<Result<List<UnitOfMeasureDto>>>;
}
