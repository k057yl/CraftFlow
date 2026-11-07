using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.RawMaterials.GetRawMaterials
{
    public record GetRawMaterialsQuery(bool IncludeArchived = false) : IRequest<Result<List<RawMaterialDto>>>;
}
