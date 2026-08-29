using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.DeleteRawMaterial;

public record DeleteRawMaterialCommand(Guid Id) : IRequest<Result>;