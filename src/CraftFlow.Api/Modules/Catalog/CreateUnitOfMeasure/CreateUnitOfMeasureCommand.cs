using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.CreateUnitOfMeasure
{
    public record CreateUnitOfMeasureCommand(string Name, string Code) : IRequest<Result<Guid>>;
}
