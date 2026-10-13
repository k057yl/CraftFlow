using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.RawMaterials.GetRawMaterials
{
    public record GetRawMaterialsQuery : IRequest<Result<List<RawMaterialDto>>>;
}
