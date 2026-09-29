using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.GetProducts
{
    public record GetProductsQuery : IRequest<Result<List<ProductDto>>>;
}
