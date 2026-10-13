using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.RawMaterials.DeleteRawMaterial;

public record DeleteRawMaterialCommand(Guid Id) : IRequest<Result>;