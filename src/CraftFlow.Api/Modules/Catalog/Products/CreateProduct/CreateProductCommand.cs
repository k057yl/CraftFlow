using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.Products.CreateProduct
{
    public record CreateProductCommand(string Name, Guid UnitOfMeasureId) : IRequest<Result<Guid>>;
}
