using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.GetRawMaterials
{
    public record GetRawMaterialsQuery : IRequest<Result<List<RawMaterialDto>>>;
}
