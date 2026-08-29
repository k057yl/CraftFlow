using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Result>;