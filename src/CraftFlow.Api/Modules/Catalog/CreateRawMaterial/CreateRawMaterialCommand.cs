using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.CreateRawMaterial
{
    public record CreateRawMaterialCommand(string Name, Guid UnitOfMeasureId) : IRequest<Result<Guid>>;
}
