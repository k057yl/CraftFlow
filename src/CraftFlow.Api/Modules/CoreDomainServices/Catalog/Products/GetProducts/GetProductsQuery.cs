using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Products.GetProducts
{
    public record GetProductsQuery : IRequest<Result<List<ProductDto>>>;
}
