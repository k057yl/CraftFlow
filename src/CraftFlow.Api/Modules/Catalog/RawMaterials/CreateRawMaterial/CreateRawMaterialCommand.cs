using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.RawMaterials.CreateRawMaterial
{
    public record CreateRawMaterialCommand(string Name, Guid UnitOfMeasureId) : IRequest<Result<Guid>>;
}
